using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Infrastructure.Users.RunescapeAccount;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUsersHandler(
    IRunescapeAccountHistoryReadStore runescapeAccountHistoryReadStore,
    IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetUsersByHistoricalRunescapeUsernameQuery, GetUsersResponse>
{
    public Task<QueryResult<GetUsersResponse>> Handle(GetUsersByHistoricalRunescapeUsernameQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.HistoricalRunescapeUsername))
            return Task.FromResult(QueryResult<GetUsersResponse>.Fail("Blank username was requested."));

        if (!runescapeAccountHistoryReadStore.TryGetUserIdsByHistoricalRunescapeUsername(query.HistoricalRunescapeUsername, out var userIds) || userIds is null)
            return Task.FromResult(QueryResult<GetUsersResponse>.Fail("User not found."));

        var users = new List<GetUserResponse>();

        foreach (var userId in userIds)
        {
            if (!userSummaryReadStore.TryGetById(userId, out var user) || user is null)
                continue;

            users.Add(GetUserResponseMapper.MapToResponse(user));
        }

        var usersReponse = new GetUsersResponse(Users: users);

        return Task.FromResult(QueryResult<GetUsersResponse>.Ok(usersReponse));
    }
}