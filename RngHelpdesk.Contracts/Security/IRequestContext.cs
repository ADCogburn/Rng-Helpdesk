using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Contracts.Security;

public interface IRequestContext
{
    Guid ActorId { get; }
    ActorType ActorType { get; }

    bool IsAuthenticated { get; }

    // null if no user (e.g. Discord Bot, WiseOldMan data input, etc.)
    AuthorityRole? AuthorityRole { get; }
}