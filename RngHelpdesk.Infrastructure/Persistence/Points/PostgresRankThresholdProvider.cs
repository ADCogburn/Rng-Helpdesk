using Microsoft.EntityFrameworkCore;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Infrastructure.Persistence.Contexts;

namespace RngHelpdesk.Infrastructure.Persistence.Points;

public sealed class PostgresRankThresholdProvider : IRankThresholdProvider
{
    private readonly AppDbContext _db;

    public PostgresRankThresholdProvider(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RankThreshold>> GetThresholdsAsync(CancellationToken ct = default)
    {
        var rows = await _db.Set<RankThresholdRow>()
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

        return rows
            .Select(x => new RankThreshold(Enum.Parse<Rank>(x.Rank), x.PointsRequired))
            .ToList();
    }
}
