using RngHelpdesk.Contracts.Security;

/// <summary>
/// Any type of entity that can perform actions in the system. This should map 1:1 with every User.
/// Each User (Domain) can have multiple Actors associated with them. This is because the Actor
/// is an Application level concept - representing the requestor and their context.
/// For example, a User may make requests from both the WebApp and the Discord Bot - these are one User, but two different Actors.
/// </summary>
public sealed class Actor
{
    public Guid ActorId { get; }
    public int UserId { get; }
    public ActorType ActorType { get; }

    public Actor(Guid id, int userId, ActorType actorType)
    {
        ActorId = id;
        UserId = userId;
        ActorType = actorType;
    }
}