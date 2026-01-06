using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Infrastructure.Common;

public class InMemEventDispatcher : IEventDispatcher
{
    private readonly IEnumerable<object> _handlers;

    public InMemEventDispatcher(IEnumerable<object> handlers)
    {
        _handlers = handlers;
    }

    public void Dispatch(IEnumerable<IDomainEvent> events)
    {
        foreach (var domainEvent in events)
        {
            DispatchSingle(domainEvent);
        }
    }

    private void DispatchSingle(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();

        foreach (var handler in _handlers)
        {
            var handlerInterfaces = handler.GetType().GetInterfaces();

            foreach (var handlerInterface in handlerInterfaces)
            {
                if (!handlerInterface.IsGenericType)
                    continue;

                if (handlerInterface.GetGenericTypeDefinition() != typeof(IProjectionHandler<>))
                    continue;

                var handledEventType = handlerInterface.GetGenericArguments()[0];

                if (handledEventType == eventType)
                {
                    handlerInterface
                        .GetMethod(nameof(IProjectionHandler<IDomainEvent>.Project))!
                        .Invoke(handler, new[] { domainEvent });
                }
            }
        }
    }
}
