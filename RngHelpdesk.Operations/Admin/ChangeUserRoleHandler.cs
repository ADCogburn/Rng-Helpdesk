using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Admin;

public sealed class ChangeUserRoleHandler(UserSummaryProjection users, IEventStore eventStore, IEventDispatcher eventDispatcher)
{
    private readonly UserSummaryProjection _usersSummaryProjection = users;
    private readonly IEventStore _eventStore = eventStore;
    private readonly IEventDispatcher _eventDispatcher = eventDispatcher;

    public async Task<CommandResult> Handle(ChangeUserRoleCommand command)
    {
        var streamHistory = await _eventStore.LoadStreamAsync("User", command.TargetUserId);

        var expectedVersion = streamHistory.Count == 0
            ? 0
            : streamHistory.Max(e => e.StreamVersion);

        var user = _usersSummaryProjection.GetSingleById(command.TargetUserId);

        if (user.AppRole == command.NewRole)
            return CommandResult.Fail("User role is already set to the requested role.");

        var ev = new UserAppRoleChangedEvent(
            ActingUserId: command.ActingUserId,
            OccurredAt: DateTimeOffset.UtcNow,
            UserId: command.TargetUserId,
            OldRole: user.AppRole,
            NewRole: command.NewRole);

        await _eventStore.AppendAsync(
            streamType: "User",
            streamId: command.TargetUserId,
            expectedVersion: expectedVersion,
            events: [ev],
            metadata: new EventStoreMetadata(
                CorrelationId: null,
                CausationId: null));

        _eventDispatcher.Dispatch([ev]);

        return CommandResult.Ok();
    }
}