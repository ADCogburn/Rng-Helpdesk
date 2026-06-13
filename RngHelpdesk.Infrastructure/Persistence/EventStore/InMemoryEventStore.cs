using RngHelpdesk.Domain.Common;
using System.Text.Json;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

public sealed class InMemoryEventStore : IEventStore
{
    private readonly List<StoredEvent> _events = [];
    private readonly object _lock = new();

    public Task<IReadOnlyList<StoredEvent>> LoadStreamAsync(
        string streamType,
        ulong streamId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var events = _events
                .Where(e => e.StreamType == streamType && e.StreamId == streamId)
                .OrderBy(e => e.StreamVersion)
                .ToList();

            return Task.FromResult<IReadOnlyList<StoredEvent>>(events);
        }
    }

    public Task<IReadOnlyList<StoredEvent>> LoadFromPositionAsync(
        long globalPosition,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var events = _events
                .Where(e => e.GlobalPosition > globalPosition)
                .OrderBy(e => e.GlobalPosition)
                .ToList();

            return Task.FromResult<IReadOnlyList<StoredEvent>>(events);
        }
    }

    public Task AppendAsync(
        string streamType,
        ulong streamId,
        int expectedVersion,
        IReadOnlyList<IEvent> events,
        EventStoreMetadata metadata,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(streamType))
            throw new ArgumentException("Stream type is required.", nameof(streamType));

        if (events.Count == 0)
            return Task.CompletedTask;

        lock (_lock)
        {
            var currentVersion = _events
                .Where(e => e.StreamType == streamType && e.StreamId == streamId)
                .Select(e => e.StreamVersion)
                .DefaultIfEmpty(0)
                .Max();

            if (currentVersion != expectedVersion)
            {
                throw new InvalidOperationException(
                    $"Concurrency conflict: Expected version {expectedVersion}, but current version is {currentVersion}.");
            }

            foreach (var e in events)
            {
                ct.ThrowIfCancellationRequested();

                var nextGlobalPosition = _events.Count + 1;
                var nextStreamVersion = currentVersion + 1;

                var stored = new StoredEvent(
                    GlobalPosition: nextGlobalPosition,
                    StreamType: streamType,
                    StreamId: streamId,
                    StreamVersion: nextStreamVersion,
                    EventType: e.GetType().AssemblyQualifiedName!, // full assembly name of event - okay for now but in real implementation, move to hard coded string and/or use some kind of mapping
                    SchemaVersion: 1,
                    OccuredAt: e.OccurredAt,
                    PayloadJson: JsonSerializer.Serialize(e, e.GetType()),
                    MetadataJson: JsonSerializer.Serialize(metadata));

                _events.Add(stored);

                currentVersion = nextStreamVersion;
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> HasAnyStreamsAsync(
        string streamType,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var hasAny = _events.Any(e => e.StreamType == streamType);
            return Task.FromResult(hasAny);
        }
    }
}