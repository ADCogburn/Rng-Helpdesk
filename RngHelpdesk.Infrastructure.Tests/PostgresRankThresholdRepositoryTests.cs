using Microsoft.EntityFrameworkCore;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Infrastructure.Persistence.Contexts;
using RngHelpdesk.Infrastructure.Persistence.Points;

namespace RngHelpdesk.Infrastructure.Tests;

public sealed class PostgresRankThresholdRepositoryTests : IClassFixture<MigrationFixture>
{
    private readonly MigrationFixture _fixture;

    public PostgresRankThresholdRepositoryTests(MigrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UpdatePointsRequiredAsync_ExistingRank_PersistsTheNewValue()
    {
        await using var db = _fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        await new PostgresRankThresholdRepository(db).UpdatePointsRequiredAsync(Rank.Iron, 12_345);

        var row = await db.Set<RankThresholdRow>().SingleAsync(x => x.Rank == nameof(Rank.Iron));
        Assert.Equal(12_345, row.PointsRequired);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task UpdatePointsRequiredAsync_RankNotInTable_Throws()
    {
        await using var db = _fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        // Administrator isn't seeded as one of the clan-point ranks.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new PostgresRankThresholdRepository(db).UpdatePointsRequiredAsync(Rank.Administrator, 100));

        await tx.RollbackAsync();
    }
}
