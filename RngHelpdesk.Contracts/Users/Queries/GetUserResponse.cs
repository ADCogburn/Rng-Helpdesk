namespace RngHelpdesk.Contracts.Users.Queries;

/// <summary>
/// The data promised to a client regarding a user.
/// </summary>
public sealed class GetUserResponse
{
    public int Id { get; init; }
    public int ClanPoints { get; init; }
    public string Rank { get; init; } = default!;
    public bool IsActive { get; init; }
    public DateTimeOffset DateCreated { get; init; }
    public GetDiscordAccountsResponse DiscordAccounts { get; init; } = new();
    public GetRunescapeAccountsResponse RunescapeAccounts { get; init; } = new();
}