namespace RngHelpdesk.Contracts.Security;

/// <summary>
/// Actor is any entity making a request to the system.
/// </summary>
public enum ActorType
{
    Unknown = 0, // unauthenticated user ("anonymous request context")
    WebUser, // from the angular web app
    DiscordUser, // from the discord bot
    Bot // the bot itself - maybe not needed?
}
