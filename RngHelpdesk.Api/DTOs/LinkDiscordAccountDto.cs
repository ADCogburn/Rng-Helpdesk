namespace RngHelpdesk.Api.DTOs;

public sealed class LinkDiscordAccountDto
{
    public ulong DiscordId { get; init; }
    public string? Username { get; init; }
}
