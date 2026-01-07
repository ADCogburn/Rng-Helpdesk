using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Contracts.Users.Commands;

public sealed class ChangeAdminStatusRequest
{
    public int UserId { get; init; }
    public AuthorityRole NewRole { get; init; }
}
