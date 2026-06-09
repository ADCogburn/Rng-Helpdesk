using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Contracts.Users.Queries;

// -- Queries for the "get user" request that returns user data --

public sealed record GetUserByIdQuery(int UserId);
public sealed record GetUserByDiscordIdQuery(ulong DiscordAccountId);
public sealed record GetUserByRunescapeUsernameQuery(string RunescapeUsername);

// -- Returned promise data --

/// <summary>
/// The promise data of a user.
/// </summary>
public sealed class GetUserResponse
{
    public int Id { get; init; }
    public AppRole AppRole { get; init; }
    public int ClanPoints { get; init; }
    public string Rank { get; init; } = default!;
    public bool IsActive { get; init; }
    public DateTimeOffset DateCreated { get; init; }
    public List<DiscordAccountView> DiscordAccounts { get; init; } = new();
    public List<RunescapeAccountView> RunescapeAccounts { get; init; } = new();
}