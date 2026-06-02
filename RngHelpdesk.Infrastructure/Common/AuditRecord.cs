using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Infrastructure.Common;

public sealed class AuditRecord
{
    public Guid Id { get; init; }

    public Guid ActorId { get; init; }
    public ActorType ActorType { get; init; } = default!;

    public string EventType { get; init; } = default!;

    public DateTimeOffset OccurredAt { get; init; }

    public string JsonPayload { get; init; } = default!;
}