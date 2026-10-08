using System.Net;
using RngHelpdesk.DiscordBot.Api;

namespace RngHelpdesk.DiscordBot.Interactions;

/// <summary>Maps API/network failures to the friendly ephemeral text shown to the Discord user.</summary>
public static class ErrorReplies
{
    public const string NotRegistered = "You're not registered with the clan helpdesk yet — ask an admin to run `/member add` for you.";
    public const string Deactivated = "Your helpdesk account is deactivated.";
    public const string NeedsAdmin = "You need an admin role in the clan helpdesk to do that.";
    public const string NotFound = "Member not found.";
    public const string Unavailable = "The helpdesk API is unavailable, try again later.";
    public const string BadRequest = "That request was rejected by the helpdesk API.";

    /// <param name="Message">Text to send to the user.</param>
    /// <param name="Unexpected">True when the failure should be logged as an error (outage, bug, misconfiguration).</param>
    public readonly record struct Reply(string Message, bool Unexpected);

    public static Reply Describe(Exception exception) => exception switch
    {
        TokenExchangeException { StatusCode: HttpStatusCode.NotFound } => new(NotRegistered, false),
        TokenExchangeException { StatusCode: HttpStatusCode.Forbidden } => new(Deactivated, false),
        ApiException { StatusCode: HttpStatusCode.Forbidden } => new(NeedsAdmin, false),
        ApiException { StatusCode: HttpStatusCode.NotFound } e => new(e.HasApiMessage ? e.Message : NotFound, false),
        ApiException { StatusCode: HttpStatusCode.BadRequest } e => new(e.HasApiMessage ? e.Message : BadRequest, false),
        // 401s here mean the bot's own credentials are wrong (user-token 401s are retried by the client).
        ApiException => new(Unavailable, true),
        HttpRequestException => new(Unavailable, true),
        TaskCanceledException => new(Unavailable, true),
        _ => new(Unavailable, true),
    };
}
