using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Contracts.Users.Commands;

public sealed record ChangeUserRoleCommand
(
    ulong ActingUserId,
    ulong TargetUserId,
    AppRole NewRole
);