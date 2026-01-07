using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Operations.Security;

public interface IActorUserResolver
{
    int? ResolveUserId(Guid actorId, ActorType actorType);
    void RegisterActor(Guid actorId, ActorType actorType, int userId);
}