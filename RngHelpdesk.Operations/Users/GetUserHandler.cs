using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Operations.Ranks;

namespace RngHelpdesk.Operations.Users;

/// <summary>
/// Get a single user by their Id, DiscordId, or Runescape username.
/// </summary>
public sealed class GetUserHandler
{
    private readonly UserPointsTotalProjection _pointsTotal;
    private readonly RankResolver _rankResolver;

    public GetUserHandler(
        UserPointsTotalProjection pointsTotal,
        RankResolver rankResolver)
    {
        _pointsTotal = pointsTotal;
        _rankResolver = rankResolver;
    }

    /// <summary>
    /// Get a single user by their Id.
    /// </summary>
    /// <param name="requestContext"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public GetUserResponse Handle(IRequestContext requestContext, GetUserByIdQuery query)
    {
        AuthorizationRules.RequireAuthentication(requestContext);

        // TEMP: fake data
        var exampleDisabledAccount = new RunescapeAccount("Disabled_Account");
        exampleDisabledAccount.Deactivate();

        var user = new User(
            id: query.userId,
            role: AuthorityRole.Member,
            discordAccounts: new[]
            {
            new DiscordAccount(123456789012345678, "FakeDiscordId"),
            new DiscordAccount(123465789012345679, "OtherFakeId", false)
            },
            runescapeAccounts: new[]
            {
            new RunescapeAccount("FakeRSN_One"),
            new RunescapeAccount("FakeRSN_Two"),
            exampleDisabledAccount
            });

        // READ-SIDE DATA
        var totalPoints = _pointsTotal.GetTotalPoints(user.Id);

        var rank = _rankResolver.Resolve(
            user.AuthorityRole,
            totalPoints
        );

        return new GetUserResponse
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
        };
    }

    /// <summary>
    /// Get a single user by their DiscordId
    /// </summary>
    /// <param name="requestContext"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetUserResponse Handle(IRequestContext requestContext, GetUserByDiscordIdQuery query)
    {
        AuthorizationRules.RequireAuthentication(requestContext);
        // Implementation would be similar to the GetUserByIdQuery handler,
        // but would retrieve the user based on the provided DiscordId.
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a single user by their Runescape username.
    /// </summary>
    /// <param name="requestContext"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetUserResponse Handle(IRequestContext requestContext, GetUserByRunescapeUsernameQuery query)
    {
        AuthorizationRules.RequireAuthentication(requestContext);
        // Implementation would be similar to the GetUserByIdQuery handler,
        // but would retrieve the user based on the provided Runescape username.
        throw new NotImplementedException();
    }
}