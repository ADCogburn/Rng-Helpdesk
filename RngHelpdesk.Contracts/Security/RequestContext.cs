using RngHelpdesk.Domain.Users;

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
    /// The administrative role of the user.
    /// </summary>
    public AuthorityRole AuthorityRole { get; init; }

    /// <summary>
    /// Has the user been authenticated by the adapter?
    /// </summary>
    public bool IsAuthenticated { get; init; }
}