using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Contracts.Users.Queries;

/// <summary>
/// The promised data of a user's list of Runescape accounts.
/// </summary>
public sealed record GetRunescapeAccountsResponse(IReadOnlyList<RunescapeAccountView> Accounts);