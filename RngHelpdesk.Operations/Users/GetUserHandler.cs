using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Contracts.Common.Ranks;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUserHandler
{
    private readonly UserSummaryProjection _users;
    private readonly UserPointsTotalProjection _pointsTotal;
    private readonly RankResolver _rankResolver;

    public GetUserHandler(
        UserSummaryProjection users,
        UserPointsTotalProjection pointsTotal,
        RankResolver rankResolver)
    {
        _users = users;
        _pointsTotal = pointsTotal;
        _rankResolver = rankResolver;
    }

    public GetUserResponse Handle(
        IRequestContext requestContext,
        GetUserByIdQuery query)
    {
        AuthorizationRules.RequireAuthentication(requestContext);

        var user = _users.GetSingleById(query.UserId);

        return MapToResponse(user);
    }

    public GetUserResponse Handle(
        IRequestContext requestContext,
        GetUserByDiscordIdQuery query)
    {
        AuthorizationRules.RequireAuthentication(requestContext);

        var user = _users.GetByDiscordId(query.DiscordAccountId);

        return MapToResponse(user);
    }

    public GetUserResponse Handle(
        IRequestContext requestContext,
        GetUserByRunescapeUsernameQuery query)
    {
        AuthorizationRules.RequireAuthentication(requestContext);

        var user = _users.GetByRunescapeUsername(query.RunescapeUsername);

        return MapToResponse(user);
    }

    private GetUserResponse MapToResponse(UserSummaryReadModel user)
    {
        var totalPoints = _pointsTotal.GetTotalPoints(user.UserId);

        var rank = _rankResolver.Resolve(
            user.AuthorityRole,
            totalPoints);

        return new GetUserResponse
        {
            Id = user.UserId,
            ClanPoints = totalPoints,
            Rank = rank.ToString(),
            IsActive = user.IsActive,
            DateCreated = user.DateCreated,

            RunescapeAccounts = user.RunescapeAccounts
                .Select(a => new RunescapeAccountView
                {
                    Username = a.Username
                })
                .ToList(),

            DiscordAccounts = user.DiscordAccounts
                .Select(d => new DiscordAccountView
                {
                    DiscordId = d.DiscordId,
                    Username = d.Username,
                    IsActive = d.IsActive
                })
                .ToList()
        };
    }
}