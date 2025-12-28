using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Operations.Common;

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent);
}
