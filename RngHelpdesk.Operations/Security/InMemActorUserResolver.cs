using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Operations.Security;

public sealed class InMemoryActorUserResolver : IActorUserResolver
{
    private readonly Dictionary<Guid, ActorIdentity> _map = new();

    public ActorIdentity? Resolve(Guid actorId)
        => _map.TryGetValue(actorId, out var identity)
            ? identity
            : null;

    public void RegisterActor(Guid actorId, ActorType actorType, int userId)
    {
        _map[actorId] = new ActorIdentity(
            actorId,
            actorType,
            userId);
    }
}

public sealed record ActorIdentity(Guid ActorId, ActorType ActorType, int UserId);