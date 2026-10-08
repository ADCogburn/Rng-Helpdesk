using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Public;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Public;

public sealed class GetPublicOverviewHandler(
    IUserSummaryReadStore userSummaryReadStore,
    IRankThresholdProvider rankThresholdProvider) : IQueryHandler<GetPublicOverviewQuery, GetPublicOverviewResponse>
{
    public async Task<QueryResult<GetPublicOverviewResponse>> Handle(GetPublicOverviewQuery query, CancellationToken cancellationToken = default)
    {
        var activeUsers = (await userSummaryReadStore.GetAllAsync(cancellationToken))
            .Where(u => u.IsActive)
            .ToList();
        var thresholds = await rankThresholdProvider.GetThresholdsAsync(cancellationToken);

        var countsByRank = activeUsers
            .GroupBy(u => u.Rank)
            .ToDictionary(g => g.Key, g => g.Count());

        return QueryResult<GetPublicOverviewResponse>.Ok(new GetPublicOverviewResponse
        {
            ActiveMemberCount = activeUsers.Count,
            TotalClanPoints = activeUsers.Sum(u => (long)u.ClanPoints),
            RankDistribution = thresholds
                .Select(t => new PublicRankCountItem(t.Rank, countsByRank.GetValueOrDefault(t.Rank)))
                .ToList()
        });
    }
}
