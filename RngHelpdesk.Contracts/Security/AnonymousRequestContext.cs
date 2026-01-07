using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Contracts.Security;

/// <summary>
/// Unauthenticated request context.
/// </summary>
public sealed class AnonymousRequestContext : IRequestContext
{
    public Guid ActorId => Guid.Empty;
    public ActorType ActorType => ActorType.Unknown;

    public bool IsAuthenticated => false;

    AuthorityRole? AuthorityRole => null;

    AuthorityRole? IRequestContext.AuthorityRole => AuthorityRole;
}