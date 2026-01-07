using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Operations.Security;

public interface IActorUserResolver
{
    int? ResolveUserId(Guid actorId, ActorType actorType);
}