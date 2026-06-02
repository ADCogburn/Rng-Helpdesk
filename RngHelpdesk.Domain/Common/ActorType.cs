namespace RngHelpdesk.Domain.Common;

/// <summary>
/// The type of actor making the request.
/// </summary>
public enum ActorType
{
    Unknown = 0, // unauthenticated user ("anonymous request context")
    User,
    System // e.g. scheduled tasks, background services, etc.
}
