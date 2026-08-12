using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Persistence.EventStore;

namespace RngHelpdesk.Infrastructure.Users;

public sealed class PostgresUserRepository : IUserRepository
{
    private const string StreamType = "User";

    private readonly IEventStore _eventStore;
    private readonly EventTypeRegistry _registry;

    public PostgresUserRepository(IEventStore eventStore, EventTypeRegistry registry)
    {
        _eventStore = eventStore;
        _registry = registry;
    }

    public async Task<bool> ExistsAsync(ulong userId, CancellationToken ct = default)
    {
        var stream = await _eventStore.LoadStreamAsync(StreamType, userId, ct);
        return stream.Count > 0;
    }

    public Task<bool> HasAnyUsersAsync(CancellationToken ct = default) =>
        _eventStore.HasAnyStreamsAsync(StreamType, ct);

    public async Task<User> GetByIdAsync(ulong userId, CancellationToken ct = default)
    {
        var stored = await _eventStore.LoadStreamAsync(StreamType, userId, ct);

        if (stored.Count == 0)
            throw new AggregateNotFoundException(nameof(User), userId);

        // The "User" stream also carries IApplicationEvents appended directly by services like
        // UserRoleService (e.g. UserAppRoleChangedEvent) that bypass the aggregate entirely -- User
        // only ever rehydrates from its own domain events, but the stream version passed in still
        // has to reflect every row physically in the stream (StreamVersion), not just the domain
        // ones, or the next SaveAsync's optimistic concurrency check would reject a legitimate
        // write as a false conflict.
        var domainEvents = stored
            .Where(e => typeof(IDomainEvent).IsAssignableFrom(_registry.GetType(e.EventType)))
            .Select(e => (IDomainEvent)Deserialize(e));

        var streamVersion = stored.Max(e => e.StreamVersion);

        return User.Rehydrate(domainEvents, streamVersion);
    }

    public async Task<IReadOnlyCollection<IDomainEvent>> SaveAsync(User user, CancellationToken ct = default)
    {
        var newEvents = user.UncommittedDomainEvents.ToArray();

        if (newEvents.Length == 0)
            return Array.Empty<IDomainEvent>();

        await _eventStore.AppendAsync(
            StreamType,
            user.Id,
            expectedVersion: user.Version,
            events: newEvents,
            metadata: new EventStoreMetadata(),
            ct: ct);

        user.ClearUncommittedDomainEvents();

        return newEvents;
    }

    public Task<bool> UserExistsWithDiscordIdAsync(ulong discordId, CancellationToken ct = default) =>
        ExistsAsync(discordId, ct);

    public async Task<bool> UserExistsWithDiscordUsernameAsync(string username, CancellationToken ct = default)
    {
        var createdEventName = _registry.GetName(typeof(UserCreatedEvent));
        var allEvents = await _eventStore.LoadFromPositionAsync(0, ct);

        return allEvents
            .Where(e => e.StreamType == StreamType && e.EventType == createdEventName)
            .Select(e => (UserCreatedEvent)Deserialize(e))
            .Any(e => e.DiscordAccount.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    private IEvent Deserialize(StoredEvent stored) =>
        StoredEventDeserializer.Deserialize(stored, _registry.GetType(stored.EventType));
}
