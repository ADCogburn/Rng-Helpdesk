//using RngHelpdesk.Contracts.Common.Ranks;
//using RngHelpdesk.Infrastructure.Persistence.Contexts;
//using RngHelpdesk.Contracts.Common.Ranks;

//namespace RngHelpdesk.Infrastructure.Persistence.Points;

//public sealed class PostgresRankThresholdProvider : IRankThresholdProvider
//{
//    private readonly AppDbContext _db;

//    public PostgresRankThresholdProvider(AppDbContext db)
//    {
//        _db = db;
//    }

//    public IReadOnlyList<RankThreshold> GetThresholds()
//    {
//        return _db.Set<RankThresholdRow>()
//            .OrderBy(x => x.SortOrder)
//            .Select(x => new RankThreshold(
//                Enum.Parse<Rank>(x.Rank),
//                x.PointsRequired))
//            .ToList();
//    }
//}

