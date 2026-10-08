using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Common.Ranks.Commands;
using RngHelpdesk.Infrastructure.Persistence.Points;
using RngHelpdesk.Operations.Admin;

namespace RngHelpdesk.Infrastructure.Tests;

/// <summary>
/// Representative of the "EF Core / AppDbContext" shape -- the only persistence mechanism besides
/// raw SQL used by any Postgres-backed class. Proves the handler's read (GetThresholdsAsync) and
/// write (UpdatePointsRequiredAsync) sides both operate against the same real seeded rank_thresholds
/// table, which RngHelpdesk.Operations.Tests can't check since its fixture uses
/// InMemoryRankThresholdProvider's hardcoded values instead of the migration's actual seed data.
/// </summary>
public sealed class UpdateRankThresholdHandlerIntegrationTests : IClassFixture<MigrationFixture>
{
    private readonly MigrationFixture _fixture;

    public UpdateRankThresholdHandlerIntegrationTests(MigrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ValidUpdateBetweenNeighbors_PersistsThroughPostgres()
    {
        await using var db = _fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        var provider = new PostgresRankThresholdProvider(db);
        var repository = new PostgresRankThresholdRepository(db);
        var handler = new UpdateRankThresholdHandler(provider, repository);

        // Steel sits between Iron (10) and Mithril (100) in the migration's seed data.
        var result = await handler.Handle(new UpdateRankThresholdCommand(Rank.Steel, 60));

        Assert.Equal(ResultStatus.Success, result.Status);

        var thresholds = await provider.GetThresholdsAsync();
        Assert.Equal(60, thresholds.Single(t => t.Rank == Rank.Steel).PointsRequired);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task Handle_PointsRequiredNotGreaterThanPreviousRank_ReturnsFailureWithoutPersisting()
    {
        await using var db = _fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        var provider = new PostgresRankThresholdProvider(db);
        var repository = new PostgresRankThresholdRepository(db);
        var handler = new UpdateRankThresholdHandler(provider, repository);

        // Steel's previous neighbor, Iron, requires 10 points.
        var result = await handler.Handle(new UpdateRankThresholdCommand(Rank.Steel, 10));

        Assert.Equal(ResultStatus.Failure, result.Status);

        var thresholds = await provider.GetThresholdsAsync();
        Assert.NotEqual(10, thresholds.Single(t => t.Rank == Rank.Steel).PointsRequired);

        await tx.RollbackAsync();
    }
}
