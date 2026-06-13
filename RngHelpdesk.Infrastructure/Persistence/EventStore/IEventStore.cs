using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

/// <summary>
/// Interface defining required behavior for storing and retrieving domain event streams.
/// </summary>
public interface IEventStore
{
    Task<IReadOnlyList<StoredEvent>> LoadStreamAsync(
        string streamType,
        ulong streamId,
        CancellationToken ct = default);

    Task<IReadOnlyList<StoredEvent>> LoadFromPositionAsync(
        long globalPosition,
        CancellationToken ct = default);

    Task AppendAsync(
        string streamType,
        ulong streamId,
        int expectedVersion,
        IReadOnlyList<IEvent> events,
        EventStoreMetadata metadata,
        CancellationToken ct = default);

    Task<bool> HasAnyStreamsAsync(string streamType, CancellationToken ct = default);
}

/// <summary>
/// A persisted event record in the EventStore.
///
/// Contains the serialized event payload along with stream, version,
/// and ordering information required for event sourcing, projection
/// rebuilding, and auditing.
/// </summary>
/// <param name="GlobalPosition"></param>
/// <param name="StreamType"></param>
/// <param name="StreamId"></param>
/// <param name="StreamVersion"></param>
/// <param name="EventType"></param>
/// <param name="SchemaVersion"></param>
/// <param name="OccuredAt"></param>
/// <param name="PayloadJson"></param>
/// <param name="MetadataJson"></param>
public sealed record StoredEvent(
    long GlobalPosition,
    string StreamType,
    ulong StreamId,
    int StreamVersion,
    string EventType,
    int SchemaVersion,
    DateTimeOffset OccuredAt,
    string PayloadJson,
    string MetadataJson
);

/// <summary>
/// Supplemental event storage metadata useful for tracing information such as correlation and
/// causation identifiers that help link related events across requests, workflows, and projections.
/// </summary>
/// <param name="CorrelationId"></param>
/// <param name="CausationId"></param>
public sealed record EventStoreMetadata(
    Guid? CorrelationId = null,
    Guid? CausationId = null
);
