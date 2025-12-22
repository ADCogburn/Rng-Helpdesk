namespace RngHelpdesk.Contracts.Security;

/// <summary>
/// Unauthenticated request context.
/// </summary>
public sealed class AnonymousRequestContext : IRequestContext
{
    public Guid ActorId => Guid.Empty;
    public ActorType ActorType => ActorType.Unknown;

    public IReadOnlySet<string> Roles => new HashSet<string>();
    public IReadOnlySet<string> Claims => new HashSet<string>();

    public bool IsAuthenticated => false;
    public bool IsMember => false;
}