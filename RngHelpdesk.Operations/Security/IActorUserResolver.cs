using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Operations.Security;

public interface IActorUserResolver
{
    ActorIdentity? Resolve(Guid actorId);
    void RegisterActor(Guid actorId, ActorType actorType, int userId);
}