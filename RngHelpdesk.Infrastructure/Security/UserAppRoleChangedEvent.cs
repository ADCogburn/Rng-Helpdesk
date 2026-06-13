using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Security;

public sealed record UserAppRoleChangedEvent(
    ulong ActingUserId,
    DateTimeOffset OccurredAt,
    ulong UserId,
    AppRole OldRole,
    AppRole NewRole
) : IApplicationEvent
{
    public static UserAppRoleChangedEvent Create(ulong actingUserId, ulong userId, AppRole oldRole, AppRole newRole)
    {
        return new UserAppRoleChangedEvent(
            ActingUserId: actingUserId,
            OccurredAt: DateTimeOffset.UtcNow,
            UserId: userId,
            OldRole: oldRole,
            NewRole: newRole
        );
    }
}