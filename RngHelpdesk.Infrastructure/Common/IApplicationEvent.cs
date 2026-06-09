using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Infrastructure.Common;

/// <summary>
/// Event that is required for event sourcing, but is not a domain event.
/// E.g. "When a user changes admin status, keep track of that record."
/// This is not a domain event because it does not represent a state change in the domain, but it is still an event that other parts of the system may be interested in.
/// </summary>
public interface IApplicationEvent : IEvent
{
}
