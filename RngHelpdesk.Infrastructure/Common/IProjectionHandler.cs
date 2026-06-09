using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Infrastructure.Common;

/// <summary>
/// Interface that declares the interest of a given Projection to the relevant event.
/// E.g. "When event TEvent that is an IDomainEvent happens, then call me."
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IProjectionHandler<in TEvent> where TEvent : IEvent
{
    void Project(TEvent @event);
}
