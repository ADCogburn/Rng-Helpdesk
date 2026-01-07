using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Operations.Security;

public sealed class InMemoryActorUserResolver : IActorUserResolver
{
    private readonly Dictionary<(Guid ActorId, ActorType ActorType), int> _map = new();

    public int? ResolveUserId(Guid actorId, ActorType actorType)
        => _map.TryGetValue((actorId, actorType), out var userId)
            ? userId
            : null;

    public void RegisterActor(Guid actorId, ActorType actorType, int userId)
    {
        _map[(actorId, actorType)] = userId;
    }
}