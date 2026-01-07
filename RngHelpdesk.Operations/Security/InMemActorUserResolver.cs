using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Operations.Security;

public sealed class InMemoryActorUserResolver : IActorUserResolver
{
    private readonly Dictionary<Guid, int> _actorToUser = new();

    public void RegisterActor(Guid actorId, int userId)
        => _actorToUser[actorId] = userId;

    public int? ResolveUserId(Guid actorId, ActorType actorType)
        => _actorToUser.TryGetValue(actorId, out var userId)
            ? userId
            : null;
}