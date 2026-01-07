using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Security;

public sealed class AuthorizationService
{
    private readonly IActorUserResolver _actorUserResolver;
    private readonly UserSummaryProjection _users;

    public AuthorizationService(
        IActorUserResolver actorUserResolver,
        UserSummaryProjection users)
    {
        _actorUserResolver = actorUserResolver;
        _users = users;
    }

    public AuthorityRole ResolveAuthority(IRequestContext context)
    {
        if (!context.IsAuthenticated)
            throw new InvalidOperationException(
                "ResolveAuthority called for unauthenticated request.");

        if (context.ActorType == ActorType.Unknown)
            throw new InvalidOperationException(
                "ResolveAuthority called with ActorType.Unknown.");

        if (context.ActorType == ActorType.Bot)
            return AuthorityRole.Administrator;

        if (context.ActorId == Guid.Empty)
            throw new InvalidOperationException(
                "Authenticated context missing ActorId.");

        var userId = _actorUserResolver.ResolveUserId(
            context.ActorId,
            context.ActorType);

        if (userId is null)
            throw new InvalidOperationException(
                $"No user mapped for actor {context.ActorId} ({context.ActorType}).");

        var user = _users.GetSingleById(userId.Value);

        return user.AuthorityRole;
    }

}
