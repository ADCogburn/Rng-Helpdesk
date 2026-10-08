using Microsoft.EntityFrameworkCore;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Infrastructure.Persistence.Contexts;

namespace RngHelpdesk.Infrastructure.Persistence.Points;

public sealed class PostgresRankThresholdRepository : IRankThresholdRepository
{
    private readonly AppDbContext _db;

    public PostgresRankThresholdRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task UpdatePointsRequiredAsync(Rank rank, int pointsRequired, CancellationToken ct = default)
    {
        var row = await _db.Set<RankThresholdRow>()
            .SingleAsync(x => x.Rank == rank.ToString(), ct);

        row.PointsRequired = pointsRequired;

        await _db.SaveChangesAsync(ct);
    }
}
