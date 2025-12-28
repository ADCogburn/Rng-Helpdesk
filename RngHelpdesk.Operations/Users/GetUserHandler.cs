using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Ranks;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Users;

/// <summary>
/// Get a single user by their Id, DiscordId, or Runescape username.
/// </summary>
public sealed class GetUserHandler
{
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

        var exampledDisabledAccount = new RunescapeAccount("Disabled_Account");
        exampledDisabledAccount.Deactivate();

        var user = new User(
            id: query.userId,
            role: AuthorityRole.Member,
            discordAccounts: new[] {
                new DiscordAccount(123456789012345678, "FakeDiscordId"),
                new DiscordAccount(123465789012345679, "OtherFakeId", false
            )},
            runescapeAccounts: new[]
            {
                new RunescapeAccount("FakeRSN_One"),
                new RunescapeAccount("FakeRSN_Two"),
                exampledDisabledAccount
            });

        // TEMP: get from db when config is setup

        var thresholds = new[]
        {
            new RankThreshold(Rank.Bronze, 0),
            new RankThreshold(Rank.Iron, 10),
            new RankThreshold(Rank.Steel, 50),
            new RankThreshold(Rank.Mithril, 100),
            new RankThreshold(Rank.Adamant, 175),
            new RankThreshold(Rank.Rune, 265),
            new RankThreshold(Rank.Dragon, 375),
        };

        var rankResolver = new RankResolver(thresholds);

        return new GetUserResponse
        {
            Id = user.Id,
            ClanPoints = user.ClanPoints,
            Rank = rankResolver.Resolve(user).ToString(),
            IsActive = user.IsActive,
            DateCreated = DateTime.Now,
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