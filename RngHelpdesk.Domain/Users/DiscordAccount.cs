using System.Text.Json.Serialization;

namespace RngHelpdesk.Domain.Users;

public sealed class DiscordAccount
{
    public ulong DiscordId { get; private set; }
    public string Username { get; private set; }

    [JsonConstructor]
    public DiscordAccount(ulong discordId, string username)
    {
        if (discordId == 0)
            throw new ArgumentNullException(nameof(discordId));

        if (username == null)
            throw new ArgumentNullException(nameof(username));

        DiscordId = discordId;
        Username = username;
    }
}
