using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Domain.Ranks;
using RngHelpdesk.Domain.Users;

public sealed class GetUserHandler
{
    public GetUserResponse Handle(int id)
    {
        // TEMP: fake data

        const ulong fakeDiscordId = 123456789012345678;

        var user = new User(
            id: id,
            role: AuthorityRole.Member,
            discordAccounts: new[] { new DiscordAccount(fakeDiscordId) },
            runescapeAccounts: new[]
            {
                new RunescapeAccount("FakeRSN_One"),
                new RunescapeAccount("FakeRSN_Two")
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
            IsActive = user.IsActive
        };
    }
}