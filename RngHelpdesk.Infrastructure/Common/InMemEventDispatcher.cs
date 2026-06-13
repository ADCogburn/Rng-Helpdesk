using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Infrastructure.Common;

public class InMemEventDispatcher : IEventDispatcher
{
    private readonly IEnumerable<object> _handlers;

    public InMemEventDispatcher(IEnumerable<object> handlers)
    {
        _handlers = handlers;
    }

    public void Dispatch(IEnumerable<IEvent> events)
    {
        foreach (var e in events)
        {
            DispatchSingle(e);
        }
    }

    private void DispatchSingle(IEvent e)
    {
        var eventType = e.GetType();

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
                        .GetMethod(nameof(IProjectionHandler<IEvent>.Project))!
                        .Invoke(handler, new[] { e });
                }
            }
        }
    }
}
