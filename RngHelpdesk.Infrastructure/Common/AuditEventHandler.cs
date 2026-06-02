using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Infrastructure.Common;

public sealed class AuditEventHandler
{
    private readonly IAuditRepository _repo;
    private readonly ISerializer _serializer;

    public AuditEventHandler(IAuditRepository repo, ISerializer serializer)
    {
        _repo = repo;
        _serializer = serializer;
    }

    public async Task Handle(IDomainEvent domainEvent)
    {
        if (domainEvent is not IAuditableEvent auditable)
            return;

        var record = new AuditRecord
        {
            Id = Guid.NewGuid(),
            ActorId = auditable.ActorId,
            ActorType = auditable.ActorType,
            OccurredAt = auditable.OccurredAt,
            EventType = domainEvent.GetType().Name,
            PayloadJson = _serializer.Serialize(domainEvent)
        };

        await _repo.Insert(record);
    }
}
