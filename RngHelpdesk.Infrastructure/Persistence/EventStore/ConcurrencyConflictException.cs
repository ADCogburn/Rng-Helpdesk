namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

/// <summary>
/// Thrown when an <see cref="IEventStore.AppendAsync"/> call targets a stream/expected-version
/// combination that another writer has already advanced past.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public string StreamType { get; }
    public ulong StreamId { get; }
    public int ExpectedVersion { get; }

    public ConcurrencyConflictException(
        string streamType,
        ulong streamId,
        int expectedVersion,
        Exception? innerException = null)
        : base(
            $"Concurrency conflict: expected version {expectedVersion} for stream {streamType}/{streamId}, but another writer had already advanced it.",
            innerException)
    {
        StreamType = streamType;
        StreamId = streamId;
        ExpectedVersion = expectedVersion;
    }
}
