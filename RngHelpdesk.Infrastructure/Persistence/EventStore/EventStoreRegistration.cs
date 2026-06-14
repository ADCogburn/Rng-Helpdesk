using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Security;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

public static class EventStoreRegistration
{
    public static EventTypeRegistry CreateRegistry()
    {
        var registry = new EventTypeRegistry();

        registry.Register<UserCreatedEvent>("User.Created");
        registry.Register<UserDeactivatedEvent>("User.Deactivated");
        registry.Register<UserReactivatedEvent>("User.Reactivated");

        registry.Register<UserAppRoleChangedEvent>("User.AppRoleChanged");

        registry.Register<RunescapeAccountLinkedEvent>("User.RunescapeAccountLinked");
        registry.Register<RunescapeAccountDelinkedEvent>("User.RunescapeAccountDelinked");
        registry.Register<RunescapeAccountRenamedEvent>("User.RunescapeAccountRenamed");

        registry.Register<ClanPointsChangedEvent>("Points.PointsChanged");

        return registry;
    }
}
