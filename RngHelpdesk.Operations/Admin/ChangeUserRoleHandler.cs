using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Services;

namespace RngHelpdesk.Operations.Admin;

public sealed class ChangeUserRoleHandler(
    UserSummaryProjection users,
    IEventDispatcher eventDispatcher,
    IUserRoleService userRoleService)
{
    private readonly UserSummaryProjection _usersSummaryProjection = users;
    private readonly IEventDispatcher _eventDispatcher = eventDispatcher;
    private readonly IUserRoleService _userRoleService = userRoleService;

    public async Task<CommandResult> Handle(ChangeUserRoleCommand command)
    {
        var user = _usersSummaryProjection.GetSingleById(command.TargetUserId);

        if (user.AppRole == command.NewRole)
            return CommandResult.Fail("User role is already set to the requested role.");

        var ev = await _userRoleService.ChangeRoleAsync(command.ActingUserId, command.TargetUserId, user.AppRole, command.NewRole);

        _eventDispatcher.Dispatch(ev);

        return CommandResult.Ok();
    }
}