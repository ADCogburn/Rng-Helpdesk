namespace RngHelpdesk.Contracts.Security;

/// <summary>
/// The type of actor making the request.
/// </summary>
public enum ActorType
{
    Unknown = 0, // unauthenticated user ("anonymous request context")
    WebUser, // from the angular web app
    DiscordUser, // from the discord bot
    Bot // the bot itself - maybe not needed?
}
