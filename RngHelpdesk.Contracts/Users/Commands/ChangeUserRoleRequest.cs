using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Contracts.Users.Commands;

public sealed class ChangeUserRoleRequest
{
    public int UserId { get; init; }
    public AppRole NewRole { get; init; }
}
