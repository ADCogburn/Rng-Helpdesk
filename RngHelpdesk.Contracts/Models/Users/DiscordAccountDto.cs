namespace RngHelpdesk.Contracts.Models.Users.Dtos;

public sealed class DiscordAccountDto
{
    public ulong DiscordId { get; init; }
    public string Username { get; init; } = string.Empty;
}