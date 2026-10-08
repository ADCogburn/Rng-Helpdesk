namespace RngHelpdesk.Api.DTOs;

public sealed class BotTokenResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
}
