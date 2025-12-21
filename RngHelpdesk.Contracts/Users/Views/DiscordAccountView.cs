namespace RngHelpdesk.Contracts.Users.Views;

public class DiscordAccountView
{
    public ulong DiscordId { get; init; }
    public string Username { get; init; } = default!;
    public bool IsActive { get; init; } = true;
}
