using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Infrastructure.Users.RunescapeAccount;

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

        var users = new List<GetUserResponse>();

        foreach (var userId in userIds)
        {
            if (!userSummaryReadStore.TryGetById(userId, out var user) || user is null)
                continue;

            users.Add(MapToResponse(user));
        }

        var usersReponse = new GetUsersResponse(Users: users);

        return QueryResult<GetUsersResponse>.Ok(usersReponse);
    }

    private GetUserResponse MapToResponse(UserSummaryReadModel user)
    {
        return new GetUserResponse(
            Id: user.UserId,
            AppRole: user.AppRole,
            ClanPoints: user.ClanPoints,
            Rank: user.Rank,
            IsActive: user.IsActive,
            DateCreated: user.DateCreated,

            RunescapeAccounts: user.RunescapeAccounts
                .Select(acc => new RunescapeAccountView(acc.Username))
                .ToList(),

            PreviousRunescapeAccounts: runescapeAccountHistoryReadStore
                .GetPreviousRunescapeAccounts(user.UserId)
                .ToList(),

            DiscordAccount: new DiscordAccountView(
                user.DiscordAccount.DiscordId,
                user.DiscordAccount.Username)
        );
    }
}