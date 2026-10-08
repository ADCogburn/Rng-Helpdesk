using Microsoft.EntityFrameworkCore;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Infrastructure.Persistence.Contexts;
using RngHelpdesk.Infrastructure.Persistence.Points;

namespace RngHelpdesk.Infrastructure.Tests;

public sealed class PostgresRankThresholdProviderTests : IClassFixture<MigrationFixture>
{
    private readonly MigrationFixture _fixture;

    public PostgresRankThresholdProviderTests(MigrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetThresholdsAsync_MapsEverySeededRow_RankAndPointsRequiredMatchTheTable()
    {
        await using var db = _fixture.CreateContext();

        var rawRows = await db.Set<RankThresholdRow>()
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        var thresholds = await new PostgresRankThresholdProvider(db).GetThresholdsAsync();

        Assert.Equal(
            rawRows.Select(r => (Rank: Enum.Parse<Rank>(r.Rank), r.PointsRequired)),
            thresholds.Select(t => (t.Rank, t.PointsRequired)));
    }

    [Fact]
    public async Task GetThresholdsAsync_OrdersBySortOrder_NotByPointsRequired()
    {
        await using var db = _fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        // Administrator isn't part of the seeded clan-point ranks, so it's a free primary key
        // here. PointsRequired is deliberately huge so that ordering by PointsRequired (wrong)
        // and ordering by SortOrder (right) would disagree on where this row lands.
        db.Set<RankThresholdRow>().Add(new RankThresholdRow
        {
            Rank = nameof(Rank.Administrator),
            PointsRequired = 999_999,
            SortOrder = -1
        });
        await db.SaveChangesAsync();

        var thresholds = await new PostgresRankThresholdProvider(db).GetThresholdsAsync();

        Assert.Equal(Rank.Administrator, thresholds.First().Rank);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task GetThresholdsAsync_ReflectsRowsEditedInTheTable_ProvingThresholdsAreLiveNotHardcoded()
    {
        await using var db = _fixture.CreateContext();
        await using var tx = await db.Database.BeginTransactionAsync();

        var ironRow = await db.Set<RankThresholdRow>().SingleAsync(x => x.Rank == nameof(Rank.Iron));
        ironRow.PointsRequired = 12_345;
        await db.SaveChangesAsync();

        var thresholds = await new PostgresRankThresholdProvider(db).GetThresholdsAsync();

        Assert.Equal(12_345, thresholds.Single(t => t.Rank == Rank.Iron).PointsRequired);

        await tx.RollbackAsync();
    }
}
