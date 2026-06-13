using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Contracts.Users.Queries;

// -- Queries for the "get user" request that returns user data --

public sealed record GetUserByIdQuery(ulong UserId);
public sealed record GetUserByRunescapeUsernameQuery(string RunescapeUsername);

// -- Returned promise data --

/// <summary>
/// The promise data of a user.
/// </summary>
public sealed record GetUserResponse(
    ulong Id,
    AppRole AppRole,
    int ClanPoints,
    Rank Rank,
    bool IsActive,
    DateTimeOffset DateCreated,
    DiscordAccountView DiscordAccount,
    List<RunescapeAccountView> RunescapeAccounts);