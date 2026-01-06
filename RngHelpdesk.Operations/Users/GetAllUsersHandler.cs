using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Operations.Ranks;

namespace RngHelpdesk.Operations.Users;

public sealed class GetAllUsersHandler
{
    private readonly UserPointsTotalProjection _pointsTotal;
    private readonly RankResolver _rankResolver;

    public GetAllUsersHandler(
        UserPointsTotalProjection pointsTotal,
        RankResolver rankResolver)
    {
        _pointsTotal = pointsTotal;
        _rankResolver = rankResolver;
    }

    public GetAllUsersResponse Handle(IRequestContext requestContext)
    {
        AuthorizationRules.RequireAdminRole(requestContext);

        // TEMP: fake user identity data
        var user = new User(
            id: 123,
            role: AuthorityRole.Member,
            discordAccounts: new[]
            {
                new DiscordAccount(123456789012345678, "FakeDiscordId"),
                new DiscordAccount(123465789012345679, "OtherFakeId", false)
            },
            runescapeAccounts: new[]
            {
                new RunescapeAccount("FakeRSN_One"),
                new RunescapeAccount("FakeRSN_Two")
            });

        // READ-SIDE DATA
        var totalPoints = _pointsTotal.GetTotalPoints(user.Id);

        var rank = _rankResolver.Resolve(
            user.AuthorityRole,
            totalPoints
        );

        return new GetAllUsersResponse
        {
            TotalCount = 1,
            Users = new[]
            {
                new GetUserResponse
                {
                    Id = user.Id,
                    ClanPoints = totalPoints,
                    Rank = rank.ToString(),
                    IsActive = user.IsActive,
                    DateCreated = user.DateCreated,

                    RunescapeAccounts = user.RunescapeAccounts
                        .Select(x => new RunescapeAccountView
                        {
                            Username = x.Username,
                            IsActive = x.IsActive
                        })
                        .ToList(),

                    DiscordAccounts = user.DiscordAccounts
                        .Select(x => new DiscordAccountView
                        {
                            DiscordId = x.DiscordId,
                            Username = x.Username,
                            IsActive = x.IsActive
                        })
                        .ToList()
                }
            }
        };
    }
}