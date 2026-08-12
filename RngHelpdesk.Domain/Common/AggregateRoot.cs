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
    /// Replays a stream that may contain IApplicationEvents interleaved with this aggregate's own
    /// IDomainEvents (e.g. a role-change event appended directly to the "User" stream, bypassing
    /// the aggregate). Only IDomainEvents are applied to state, but Version still advances for
    /// every event in the stream -- it has to match the event store's real StreamVersion counter,
    /// which counts every append regardless of event kind, or the next SaveAsync's optimistic
    /// concurrency check would reject a legitimate write as a false conflict.
    /// </summary>
    public void LoadFromHistory(IEnumerable<IEvent> events)
    {
        foreach (var e in events)
        {
            if (e is IDomainEvent domainEvent)
                Apply(domainEvent);

            Version++;
        }
    }

    protected abstract void Apply(IDomainEvent domainEvent);
}
