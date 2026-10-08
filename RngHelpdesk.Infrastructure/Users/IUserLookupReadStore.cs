namespace RngHelpdesk.Infrastructure.Users;

/// <summary>
/// Defines access to the UserSummaryProjeection for checking values of a user.
/// </summary>
public interface IUserLookupReadStore
{
    Task<bool> ExistsWithDiscordIdAsync(ulong discordId, CancellationToken ct = default);
    Task<bool> ExistsWithDiscordUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsWithRunescapeUsernameAsync(string rsn, CancellationToken ct = default);
}
