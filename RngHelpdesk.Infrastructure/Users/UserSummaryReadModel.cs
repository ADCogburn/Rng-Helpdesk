using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Infrastructure.Users;

/// <summary>
/// Summary record of a user (including both domain and infra concerns, like points and app role respetively).
/// </summary>
/// <param name="UserId"></param>
/// <param name="IsActive"></param>
/// <param name="AppRole"></param>
/// <param name="Rank"></param>
/// <param name="DateCreated"></param>
/// <param name="ClanPoints"></param>
/// <param name="RunescapeAccounts"></param>
/// <param name="DiscordAccount"></param>
public sealed record UserSummaryReadModel(
    string DisplayName,
    ulong UserId,
    bool IsActive,
    AppRole AppRole,
    Rank Rank,
    DateTimeOffset DateCreated,
    int ClanPoints,
    IReadOnlyList<RunescapeAccountView> RunescapeAccounts,
    DiscordAccountView DiscordAccount);