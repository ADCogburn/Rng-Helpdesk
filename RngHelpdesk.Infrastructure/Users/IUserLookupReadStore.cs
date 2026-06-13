namespace RngHelpdesk.Infrastructure.Users;

/// <summary>
/// Defines access to the UserSummaryProjeection for checking values of a user.
/// </summary>
public interface IUserLookupReadStore
{
    bool ExistsWithDiscordId(ulong discordId);
    bool ExistsWithDiscordUsername(string username);
    bool ExistsWithRunescapeUsername(string rsn);
}
