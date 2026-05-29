using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using System.Text.Json;

namespace RngHelpdesk.Infrastructure.Users;

public sealed class PostgresUserRepository : IUserRepository
{
    private readonly IEventStore _eventStore;
    private readonly EventTypeRegistry _registry;
    private readonly IEventStoreMetadataProvider _metadataProvider;

    public PostgresUserRepository(
        IEventStore eventStore,
        EventTypeRegistry registry,
        IEventStoreMetadataProvider metadataProvider)
    {
        _eventStore = eventStore;
        _registry = registry;
        _metadataProvider = metadataProvider;
    }

    public bool Exists(int userId)
    {
        var events = _eventStore
            .LoadStreamAsync("User", userId)
            .GetAwaiter()
            .GetResult();

        return events.Count > 0;
    }

    public bool HasAnyUsers() =>
        _eventStore.HasAnyStreamsAsync("User").GetAwaiter().GetResult();

    public User GetById(int userId)
    {
        var storedEvents = _eventStore
            .LoadStreamAsync("User", userId)
            .GetAwaiter()
            .GetResult();

        if (storedEvents.Count == 0)
            throw new InvalidOperationException($"User {userId} not found");

        var domainEvents = Deserialize(storedEvents);

        return User.Rehydrate(domainEvents);
    }

    public IReadOnlyCollection<IDomainEvent> Save(User user)
    {
        var newEvents = user.UncommittedDomainEvents.ToArray();

        if (newEvents.Length == 0)
            return Array.Empty<IDomainEvent>();

        var expectedVersion = user.Version; // or however you track loaded version

        _eventStore.AppendAsync(
                "User",
                user.Id,
                expectedVersion,
                newEvents,
                _metadataProvider.GetMetadata())
            .GetAwaiter()
            .GetResult();

        user.ClearUncommittedDomainEvents();

        return newEvents;
    }

    private IReadOnlyList<IDomainEvent> Deserialize(
        IReadOnlyList<StoredEvent> stored)
    {
        var list = new List<IDomainEvent>(stored.Count);

        foreach (var e in stored)
        {
            var clrType = _registry.GetType(e.EventType);

            var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(
                e.PayloadJson,
                clrType)!;

            list.Add(domainEvent);
        }

        return list;
    }
}