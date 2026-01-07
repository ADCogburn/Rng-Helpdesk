using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class AuthorityRoleChangedEvent : IDomainEvent
{
    public int UserId { get; }
    public AuthorityRole OldRole { get; }
    public AuthorityRole NewRole { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public AuthorityRoleChangedEvent(
        int userId,
        AuthorityRole oldRole,
        AuthorityRole newRole)
    {
        if (oldRole == newRole)
            throw new DomainException("Authority role did not change.");

        UserId = userId;
        OldRole = oldRole;
        NewRole = newRole;
    }
}

