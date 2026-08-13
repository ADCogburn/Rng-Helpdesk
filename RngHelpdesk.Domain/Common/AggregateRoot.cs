using RngHelpdesk.Domain.Common;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _uncommittedDomainEvents = new();
    public IReadOnlyCollection<IDomainEvent> UncommittedDomainEvents => _uncommittedDomainEvents;
    public int Version { get; protected set; }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        Apply(domainEvent);
        _uncommittedDomainEvents.Add(domainEvent);
    }

    public void ClearUncommittedDomainEvents()
    {
        _uncommittedDomainEvents.Clear();
    }

    /// <summary>
    /// Replays this aggregate's own domain events. streamVersion is the event store's real
    /// StreamVersion for the underlying stream, which the caller must supply explicitly rather than
    /// have it inferred from domainEvents.Count() -- the stream may also carry IApplicationEvents
    /// that bypass the aggregate entirely (e.g. a role-change event appended directly to the "User"
    /// stream), so its true length can exceed the number of domain events being replayed here.
    /// Version has to match that real stream position, or the next SaveAsync's optimistic
    /// concurrency check would reject a legitimate write as a false conflict.
    /// </summary>
    public void LoadFromHistory(IEnumerable<IDomainEvent> domainEvents, int streamVersion)
    {
        foreach (var domainEvent in domainEvents)
        {
            Apply(domainEvent);
        }

        Version = streamVersion;
    }

    protected abstract void Apply(IDomainEvent domainEvent);
}
