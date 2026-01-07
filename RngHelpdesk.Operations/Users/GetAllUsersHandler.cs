using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Ranks;

namespace RngHelpdesk.Operations.Users;

public sealed class GetAllUsersHandler
{
    private readonly UserSummaryProjection _users;
    private readonly UserPointsTotalProjection _points;
    private readonly RankResolver _rankResolver;

    public GetAllUsersHandler(
        UserSummaryProjection users,
        UserPointsTotalProjection points,
        RankResolver rankResolver)
    {
        _users = users;
        _points = points;
        _rankResolver = rankResolver;
    }

    public GetAllUsersResponse Handle(IRequestContext requestContext)
    {
        AuthorizationRules.RequireAdminRole(requestContext);

        var users = _users.GetAll();

        return new GetAllUsersResponse
        {
            TotalCount = users.Count,
            Users = users.Select(u =>
            {
                var totalPoints = _points.GetTotalPoints(u.UserId);
                var rank = _rankResolver.Resolve(
                    u.AuthorityRole,
                    totalPoints
                );

                return new GetUserResponse
                {
                    Id = u.UserId,
                    ClanPoints = totalPoints,
                    Rank = rank.ToString(),
                    IsActive = u.IsActive,
                    DateCreated = u.DateCreated,
                    RunescapeAccounts = u.RunescapeAccounts.ToList(),
                    DiscordAccounts = u.DiscordAccounts.ToList()
                };
            }).ToList()
        };
    }
}