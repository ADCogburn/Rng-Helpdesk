using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Contracts.Security;

/// <summary>
/// Unauthenticated request context.
/// </summary>
public sealed class AnonymousRequestContext : IRequestContext
{
    public Guid ActorId => Guid.Empty;
    public ActorType ActorType => ActorType.Unknown;

    public AuthorityRole AuthorityRole => AuthorityRole.Guest;

    public bool IsAuthenticated => false;
}