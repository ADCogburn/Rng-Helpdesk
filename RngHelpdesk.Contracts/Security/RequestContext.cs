namespace RngHelpdesk.Contracts.Security;

/// <summary>
/// Context of the request made to the application.
/// </summary>
public sealed class RequestContext : IRequestContext
{
    public int UserId { get; init; }
}