using RngHelpdesk.Domain.Users.Events;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

public static class EventStoreRegistration
{
    public static EventTypeRegistry CreateRegistry()
    {
        var registry = new EventTypeRegistry();

        /// --- User ---

        registry.Register<UserCreatedEvent>("User.Created");
        registry.Register<UserDeactivatedEvent>("User.Deactivated");
        registry.Register<UserReactivatedEvent>("User.Reactivated");

        registry.Register<AuthorityRoleChangedEvent>("User.AuthorityRoleChanged");

        registry.Register<RunescapeAccountLinkedEvent>("User.RunescapeAccountLinked");
        registry.Register<RunescapeAccountDelinkedEvent>("User.RunescapeAccountDelinked");
        registry.Register<RunescapeAccountRenamedEvent>("User.RunescapeAccountRenamed");

        registry.Register<DiscordAccountLinkedEvent>("User.DiscordAccountLinked");
        registry.Register<DiscordAccountDelinkedEvent>("User.DiscordAccountDelinked");

        /// --- Points ---

        registry.Register<ClanPointsChangedEvent>("Points.PointsChanged");

        return registry;
    }
}
