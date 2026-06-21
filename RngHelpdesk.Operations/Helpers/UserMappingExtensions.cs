using RngHelpdesk.Contracts.Models.Users;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Helpers;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this UserSummaryReadModel user)
    {
        return new UserDto(
            UserId: user.UserId,
            AppRole: user.AppRole,
            ClanPoints: user.ClanPoints,
            Rank: user.Rank,
            IsActive: user.IsActive,
            DateCreated: user.DateCreated,
            DiscordAccount: new DiscordAccountView(
                user.DiscordAccount.DiscordId,
                user.DiscordAccount.Username),
            RunescapeAccounts: user.RunescapeAccounts
                .Select(a => new RunescapeAccountView(a.Username))
                .ToList());
    }
}
