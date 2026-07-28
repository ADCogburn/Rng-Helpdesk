using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;
using RngHelpdesk.Operations.Services;

namespace RngHelpdesk.Operations.Admin;

public sealed class ChangeUserRoleHandler(
    IUserRoleService userRoleService,
    IUserSummaryReadStore userSummaryReadStore,
    IEventDispatcher eventDispatcher) : ICommandHandler<ChangeUserRoleCommand>
{
    public async Task<CommandResult> Handle(ChangeUserRoleCommand command, CancellationToken cancellationToken = default)
    {
        if (!userSummaryReadStore.TryGetById(command.TargetUserId, out var user) || user is null)
            return CommandResult.Fail("User not found.");

        if (user.AppRole == command.NewRole)
            return CommandResult.Fail("User role is already set to the requested role.");

        var ev = await userRoleService.ChangeRoleAsync(command.ActingUserId, command.TargetUserId, user.AppRole, command.NewRole);

        eventDispatcher.Dispatch(ev);

        return CommandResult.Ok();
    }
}