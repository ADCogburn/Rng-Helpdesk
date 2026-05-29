namespace RngHelpdesk.Contracts.Discord;

/// <summary>
/// Resolves a Discord user ID to their display username via an external service (e.g. DiscordBot).
/// </summary>
public interface IDiscordUsernameResolver
{
    /// <summary>
    /// Fetches the Discord username for the given user ID.
    /// Returns null if the user cannot be resolved (e.g. not in shared guild, service unavailable).
    /// </summary>
    Task<string?> ResolveUsernameAsync(ulong discordId, CancellationToken ct = default);
}
