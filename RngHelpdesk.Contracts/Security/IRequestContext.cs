namespace RngHelpdesk.Contracts.Security;

public interface IRequestContext
{
    Guid ActorId { get; }
    ActorType ActorType { get; }

    IReadOnlySet<string> Roles { get; }
    IReadOnlySet<string> Claims { get; }

    bool IsAuthenticated { get; }
    bool IsMember { get; }
}