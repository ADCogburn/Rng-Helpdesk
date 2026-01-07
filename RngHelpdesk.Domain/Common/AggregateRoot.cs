using RngHelpdesk.Domain.Common;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _uncommittedDomainEvents = new();
    public IReadOnlyCollection<IDomainEvent> UncommittedDomainEvents => _uncommittedDomainEvents;

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        Apply(domainEvent);
        _uncommittedDomainEvents.Add(domainEvent);
    }

    public void ClearUncommittedDomainEvents()
    {
        _uncommittedDomainEvents.Clear();
    }

    public void LoadFromHistory(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            Apply(domainEvent);
        }
    }

    protected abstract void Apply(IDomainEvent domainEvent);
}
