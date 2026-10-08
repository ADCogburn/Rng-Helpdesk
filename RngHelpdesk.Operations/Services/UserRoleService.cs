using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using RngHelpdesk.Infrastructure.Security;

namespace RngHelpdesk.Operations.Services;

public sealed class UserRoleService(IEventStore eventStore) : IUserRoleService
{
    private readonly IEventStore _eventStore = eventStore;

    public async Task<IReadOnlyCollection<IApplicationEvent>> ChangeRoleAsync(
        ulong actingUserId,
        ulong userId,
        AppRole oldRole,
        AppRole newRole,
        CancellationToken ct = default)
    {
        var streamHistory = await _eventStore.LoadStreamAsync("User", userId, ct);

        var expectedVersion = streamHistory.Count == 0
            ? 0
            : streamHistory.Max(e => e.StreamVersion);

        var ev = UserAppRoleChangedEvent.Create(
            actingUserId: actingUserId,
            userId: userId,
            oldRole: oldRole,
            newRole: newRole);

        await _eventStore.AppendAsync(
            streamType: "User",
            streamId: userId,
            expectedVersion: expectedVersion,
            events: [ev],
            metadata: new EventStoreMetadata(null, null),
            ct: ct);

        return [ev];
    }
}