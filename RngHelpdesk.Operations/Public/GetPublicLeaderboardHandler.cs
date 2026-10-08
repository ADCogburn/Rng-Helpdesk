using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Public;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Public;

public sealed class GetPublicLeaderboardHandler(IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetPublicLeaderboardQuery, GetPublicLeaderboardResponse>
{
    public async Task<QueryResult<GetPublicLeaderboardResponse>> Handle(GetPublicLeaderboardQuery query, CancellationToken cancellationToken = default)
    {
        var top = Math.Clamp(query.Top, 1, GetPublicLeaderboardQuery.MaxTop);

        var allUsers = await userSummaryReadStore.GetAllAsync(cancellationToken);

        var entries = allUsers
            .Where(u => u.IsActive && u.RunescapeAccounts.Count > 0)
            .Select(u => new { u.ClanPoints, u.Rank, Rsn = u.RunescapeAccounts[0].Username })
            .OrderByDescending(u => u.ClanPoints)
            .ThenBy(u => u.Rsn, StringComparer.OrdinalIgnoreCase)
            .Take(top)
            .Select((u, i) => new PublicLeaderboardItem(i + 1, u.Rsn, u.Rank, u.ClanPoints))
            .ToList();

        return QueryResult<GetPublicLeaderboardResponse>.Ok(new GetPublicLeaderboardResponse
        {
            Entries = entries
        });
    }
}
