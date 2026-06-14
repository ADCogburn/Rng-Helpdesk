using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUserHandler(IUserSummaryReadStore userSummaryReadStore)
{
    public QueryResult<GetUserResponse> Handle(GetUserByIdQuery query)
    {
        if (!userSummaryReadStore.TryGetById(query.UserId, out var user) || user is null)
            return QueryResult<GetUserResponse>.Fail("User not found.");

        return QueryResult<GetUserResponse>.Ok(MapToResponse(user));
    }

    public QueryResult<GetUserResponse> Handle(GetUserByRunescapeUsernameQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.RunescapeUsername))
            return QueryResult<GetUserResponse>.Fail("Blank username was requested.");

        if (!userSummaryReadStore.TryGetByRunescapeUsername(query.RunescapeUsername, out var user) || user is null)
            return QueryResult<GetUserResponse>.Fail("User not found.");

        return QueryResult<GetUserResponse>.Ok(MapToResponse(user));
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

            DiscordAccount: new DiscordAccountView(
                user.DiscordAccount.DiscordId,
                user.DiscordAccount.Username)
        );
    }
}