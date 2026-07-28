using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users;

public static class GetUserResponseMapper
{
    public static GetUserResponse MapToResponse(UserSummaryReadModel user)
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
