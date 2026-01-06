using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Operations.Common;

public interface IProjectionHandler<in TEvent> where TEvent : IDomainEvent
{
    void Handle(TEvent domainEvent);
}
