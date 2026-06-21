using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Models.Users;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Infrastructure.Users.RunescapeAccount;
using RngHelpdesk.Operations.Helpers;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUsersHandler(
    IRunescapeAccountHistoryReadStore runescapeAccountHistoryReadStore,
    IUserSummaryReadStore userSummaryReadStore)
{
    public QueryResult<GetUsersResponse> Handle(GetUsersByHistoricalRunescapeUsernameQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.HistoricalRunescapeUsername))
            return QueryResult<GetUsersResponse>.Fail("Blank username was requested.");

        if (!runescapeAccountHistoryReadStore.TryGetUserIdsByHistoricalRunescapeUsername(query.HistoricalRunescapeUsername, out var userIds) || userIds is null)
            return QueryResult<GetUsersResponse>.Fail("User not found.");

        var users = new List<UserDto>();

        foreach (var userId in userIds)
        {
            if (!userSummaryReadStore.TryGetById(userId, out var user) || user is null)
                continue;

            users.Add(user.ToDto());
        }

        return QueryResult<GetUsersResponse>.Ok(new GetUsersResponse(users));
    }
}