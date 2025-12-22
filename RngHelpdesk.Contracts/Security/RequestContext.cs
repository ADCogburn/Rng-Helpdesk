namespace RngHelpdesk.Contracts.Security;

/// <summary>
/// Entire context of a request going into the handlers.
/// </summary>
public sealed class RequestContext : IRequestContext
{
    /// <summary>
    /// Id of the requesting entity.
    /// </summary>
    public Guid ActorId { get; init; }

    /// <summary>
    /// Type of the requesting entity.
    /// </summary>
    public ActorType ActorType { get; init; }

    /// <summary>
    /// The role(s) the user has. Currently only one is used, but leaving it open for flexibility.
    /// </summary>
    public IReadOnlySet<string> Roles { get; init; } = new HashSet<string>();
    /// <summary>
    /// The claim(s) the user has.
    /// </summary>
    public IReadOnlySet<string> Claims { get; init; } = new HashSet<string>();

    /// <summary>
    /// Has the user been authenticated by the adapter?
    /// </summary>
    public bool IsAuthenticated { get; init; }
    /// <summary>
    /// Is this user a verified member of the clan?
    /// </summary>
    public bool IsMember { get; init; }
}