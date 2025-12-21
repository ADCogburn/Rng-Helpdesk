namespace RngHelpdesk.Contracts.Users;

/// <summary>
/// The data promised to a client regarding a user.
/// </summary>
public sealed class UserResponse
{
    public int Id { get; init; }
    public int ClanPoints { get; init; }
    public string Rank { get; init; } = default!;
    public bool IsActive { get; init; }
    public DateTimeOffset DateCreated { get; init; }
    public IReadOnlyList<DiscordAccountResponse> DiscordAccounts { get; init; } = [];
    public IReadOnlyList<RunescapeAccountResponse> RunescapeAccounts { get; init; } = [];
}