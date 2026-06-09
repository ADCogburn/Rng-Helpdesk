using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Infrastructure.Users;

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

    public GetUserResponse Handle(GetUserByIdQuery query)
    {
        var user = _users.GetSingleById(query.UserId);

        return MapToResponse(user);
    }

    public GetUserResponse Handle(GetUserByDiscordIdQuery query)
    {
        var user = _users.GetByDiscordId(query.DiscordAccountId);

        return MapToResponse(user);
    }

    public GetUserResponse Handle(GetUserByRunescapeUsernameQuery query)
    {
        var user = _users.GetByRunescapeUsername(query.RunescapeUsername);

        return MapToResponse(user);
    }

    private GetUserResponse MapToResponse(UserSummaryReadModel user)
    {
        var totalPoints = _pointsTotal.GetTotalPoints(user.UserId);

        var rank = _rankResolver.Resolve(
            user.AppRole,
            totalPoints);

        return new GetUserResponse
        {
            Id = user.UserId,
            AppRole = user.AppRole,
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

            DiscordAccounts = user.DiscordAccount
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