using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

/// <summary>
/// Interface defining required behavior for storing and retrieving domain event streams.
/// </summary>
public interface IEventStore
{
    Task<IReadOnlyList<StoredEvent>> LoadStreamAsync(
        string streamType,
        int streamId,
        CancellationToken ct = default);

    Task<IReadOnlyList<StoredEvent>> LoadFromPositionAsync(
        long globalPosition,
        CancellationToken ct = default);

    Task AppendAsync(
        string streamType,
        int streamId,
        int expectedVersion,
        IReadOnlyList<IDomainEvent> events,
        EventStoreMetadata metadata,
        CancellationToken ct = default);

    Task<bool> HasAnyStreamsAsync(string streamType, CancellationToken ct = default);
}

/// <summary>
/// Persisted domain event.
/// </summary>
/// <param name="GlobalPosition"></param>
/// <param name="StreamType"></param>
/// <param name="StreamId"></param>
/// <param name="StreamVersion"></param>
/// <param name="EventType"></param>
/// <param name="SchemaVersion"></param>
/// <param name="OccurredUtc"></param>
/// <param name="PayloadJson"></param>
/// <param name="MetadataJson"></param>
public sealed record StoredEvent(
    long GlobalPosition,
    string StreamType,
    int StreamId,
    int StreamVersion,
    string EventType,
    int SchemaVersion,
    DateTime OccurredUtc,
    string PayloadJson,
    string MetadataJson
);

/// <summary>
/// Data about the initiator of a command.
/// </summary>
/// <param name="ActorId"></param>
/// <param name="ActorType"></param>
/// <param name="CorrelationId"></param>
/// <param name="CausationId"></param>
public sealed record EventStoreMetadata(
    Guid? ActorId,
    string? ActorType,
    Guid? CorrelationId,
    Guid? CausationId
);
