//using Microsoft.Extensions.DependencyInjection;
//using RngHelpdesk.Contracts.Common.Ranks;

//namespace RngHelpdesk.Infrastructure.Persistence.Points;

///// <summary>
///// Singleton-compatible provider that caches rank thresholds.
///// Uses IServiceScopeFactory to resolve a scoped IRankThresholdProvider and cache the result.
///// </summary>
//public sealed class CachingRankThresholdProvider : IRankThresholdProvider
//{
//    private readonly IServiceScopeFactory _scopeFactory;
//    private volatile IReadOnlyList<RankThreshold>? _cache;

//    public CachingRankThresholdProvider(IServiceScopeFactory scopeFactory)
//    {
//        _scopeFactory = scopeFactory;
//    }

//    public IReadOnlyList<RankThreshold> GetThresholds()
//    {
//        if (_cache is not null)
//            return _cache;

//        lock (this)
//        {
//            if (_cache is not null)
//                return _cache;

//            using var scope = _scopeFactory.CreateScope();
//            var inner = scope.ServiceProvider.GetRequiredService<PostgresRankThresholdProvider>();
//            _cache = inner.GetThresholds();

//            return _cache;
//        }
//    }
//}
