namespace RngHelpdesk.Domain.Users;

public sealed class DiscordAccount
{
    public int Id { get; set; }
    public ulong DiscordId { get; set; }

    public DiscordAccount(ulong discordId)
    {
        DiscordId = discordId;
    }
}
