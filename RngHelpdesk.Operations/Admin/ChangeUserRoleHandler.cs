using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Security;

namespace RngHelpdesk.Operations.Admin;

public sealed class ChangeUserRoleHandler
{
    private readonly IAuthStore _authStore;

    public ChangeUserRoleHandler(IAuthStore authStore)
    {
        _authStore = authStore;
    }

    public CommandResult Handle(
        IRequestContext context,
        ChangeUserRoleRequest request)
    {
        _authStore.ChangeRole(request.UserId, request.NewRole);

        return CommandResult.Ok();
    }
}