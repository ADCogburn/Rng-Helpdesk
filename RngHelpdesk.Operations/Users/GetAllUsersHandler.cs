using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class GetAllUsersHandler
{
    private readonly ICredentialStore _authStore;
    private readonly UserSummaryProjection _users;
    private readonly UserPointsTotalProjection _points;
    private readonly RankResolver _rankResolver;

    public GetAllUsersHandler(
        ICredentialStore authStore,
        UserSummaryProjection users,
        UserPointsTotalProjection points,
        RankResolver rankResolver)
    {
        _authStore = authStore;
        _users = users;
        _points = points;
        _rankResolver = rankResolver;
    }

    public GetAllUsersResponse Handle()
    {
        var users = _users.GetAll();

        return new GetAllUsersResponse
        {
            TotalCount = users.Count,
            Users = users.Select(u =>
            {
                var totalPoints = _points.GetTotalPoints(u.UserId);
                var appRole = _authStore.GetAppRole(u.UserId);
                var rank = _rankResolver.Resolve(
                    appRole,
                    totalPoints
                );

                return new GetUserResponse
                {
                    Id = u.UserId,
                    AppRole = appRole,
                    ClanPoints = totalPoints,
                    Rank = rank.ToString(),
                    IsActive = u.IsActive,
                    DateCreated = u.DateCreated,
                    RunescapeAccounts = u.RunescapeAccounts.ToList(),
                    DiscordAccounts = u.DiscordAccount.ToList()
                };
            }).ToList()
        };
    }
}