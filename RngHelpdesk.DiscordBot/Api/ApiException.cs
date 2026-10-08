using System.Net;

namespace RngHelpdesk.DiscordBot.Api;

/// <summary>A non-success response from the RngHelpdesk API. <see cref="Message"/> is the API's own message, if any.</summary>
public class ApiException(HttpStatusCode statusCode, string? message = null) : Exception(message ?? $"API returned {(int)statusCode}.")
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    /// <summary>True when the API supplied a message body (as opposed to our fallback text).</summary>
    public bool HasApiMessage { get; } = !string.IsNullOrWhiteSpace(message);
}

/// <summary>Failure of the Discord-id → user-token exchange (404 = not registered, 403 = deactivated).</summary>
public sealed class TokenExchangeException(HttpStatusCode statusCode, string? message = null) : ApiException(statusCode, message);
