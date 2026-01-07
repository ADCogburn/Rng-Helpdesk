using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Contracts.Security;

public interface IRequestContext
{
    Guid ActorId { get; }
    ActorType ActorType { get; }

    AuthorityRole AuthorityRole { get; }

    bool IsAuthenticated { get; }
}