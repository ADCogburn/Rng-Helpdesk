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
        if (!context.IsAuthenticated ||
            context.ActorType == ActorType.Unknown)
        {
            return AuthorityRole.Guest;
        }

        if (context.ActorType == ActorType.Bot)
        {
            return AuthorityRole.Administrator;
        }

        var userId = _actorUserResolver.ResolveUserId(
            context.ActorId,
            context.ActorType);

        if (userId is null)
        {
            return AuthorityRole.Guest;
        }

        var user = _users.GetSingleById(userId.Value);

        return user.AuthorityRole;
    }
}
