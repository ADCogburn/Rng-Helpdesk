using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Infrastructure.Common;

/// <summary>
/// Given some events, deliver them to all those who are interested.
/// </summary>
public interface IEventDispatcher
{
    void Dispatch(IEnumerable<IEvent> events);
}
