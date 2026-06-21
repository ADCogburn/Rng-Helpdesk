using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Contracts.Models.Users;

public sealed record UserDto(
    string DisplayName,
    ulong UserId,
    AppRole AppRole,
    Rank Rank,
    int ClanPoints,
    bool IsActive,
    DateTimeOffset DateCreated,
    DiscordAccountView DiscordAccount,
    IReadOnlyList<RunescapeAccountView> RunescapeAccounts);
