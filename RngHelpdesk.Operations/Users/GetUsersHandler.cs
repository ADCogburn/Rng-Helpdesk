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
    public async Task<QueryResult<GetUsersResponse>> Handle(GetUsersByHistoricalRunescapeUsernameQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.HistoricalRunescapeUsername))
            return QueryResult<GetUsersResponse>.Fail("Blank username was requested.");

        var userIds = await runescapeAccountHistoryReadStore.GetUserIdsByHistoricalRunescapeUsernameAsync(query.HistoricalRunescapeUsername, cancellationToken);

        if (userIds is null)
            return QueryResult<GetUsersResponse>.Fail("User not found.");

        var users = new List<GetUserResponse>();

        foreach (var userId in userIds)
        {
            var user = await userSummaryReadStore.GetByIdAsync(userId, cancellationToken);

            if (user is null)
                continue;

            users.Add(GetUserResponseMapper.MapToResponse(user));
        }

        var usersReponse = new GetUsersResponse(Users: users);

        return QueryResult<GetUsersResponse>.Ok(usersReponse);
    }
}