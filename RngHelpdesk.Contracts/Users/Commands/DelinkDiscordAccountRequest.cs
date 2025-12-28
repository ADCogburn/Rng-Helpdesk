namespace RngHelpdesk.Contracts.Users.Commands;

public sealed class DelinkDiscordAccountRequest
{
    public int UserId { get; init; }
    public ulong DiscordId { get; init; }
}
