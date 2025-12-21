namespace RngHelpdesk.Domain.Users;

public sealed class DiscordAccount
{
    public int Id { get; set; }
    public ulong DiscordId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public DiscordAccount(ulong discordId, string username, bool isActive = true)
    {
        DiscordId = discordId;
        Username = username;
        IsActive = isActive;
    }
}
