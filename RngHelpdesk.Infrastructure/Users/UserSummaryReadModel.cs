using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Infrastructure.Users;

public sealed record UserSummaryReadModel
(
    ulong UserId,
    bool IsActive,
    AppRole AppRole,
    Rank Rank,
    DateTimeOffset DateCreated,
    int ClanPoints,
    IReadOnlyList<RunescapeAccountView> RunescapeAccounts,
    DiscordAccountView DiscordAccount
);