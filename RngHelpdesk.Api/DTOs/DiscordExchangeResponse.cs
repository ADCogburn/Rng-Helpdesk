using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Api.DTOs;

public sealed class DiscordExchangeResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public AppRole AppRole { get; init; }
}
