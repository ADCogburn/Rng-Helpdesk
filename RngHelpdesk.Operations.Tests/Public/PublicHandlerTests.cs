using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Public;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Operations.Public;

namespace RngHelpdesk.Operations.Tests.Public;

public class PublicHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private async Task<User> CreateMemberAsync(ulong discordId, string? rsn, int points = 0, bool active = true)
    {
        var accounts = rsn is null ? [] : new[] { new RunescapeAccount(rsn) };
        var user = await _fixture.CreateAndDispatchUserAsync(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(discordId, $"discord{discordId}"),
            accounts);

        if (points > 0)
            user.AddClanPoints(TestUsers.DefaultActingUserId, points, "test");
        if (!active)
            user.Deactivate(TestUsers.DefaultActingUserId);

        if (points > 0 || !active)
            _fixture.EventDispatcher.Dispatch(await _fixture.UserRepository.SaveAsync(user));

        return user;
    }

    // -- Overview --

    [Fact]
    public async Task Overview_NoUsers_ReturnsZeroesAndEveryPointBasedRank()
    {
        var handler = new GetPublicOverviewHandler(_fixture.UserSummaryProjection, _fixture.RankThresholdProvider);

        var result = await handler.Handle(new GetPublicOverviewQuery());

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(0, result.Value!.ActiveMemberCount);
        Assert.Equal(0, result.Value.TotalClanPoints);
        Assert.Equal(14, result.Value.RankDistribution.Count);
        Assert.All(result.Value.RankDistribution, r => Assert.Equal(0, r.Count));
    }

    [Fact]
    public async Task Overview_CountsActiveUsersOnly()
    {
        await CreateMemberAsync(101, "Alpha", points: 100);
        await CreateMemberAsync(102, "Bravo");
        await CreateMemberAsync(103, "Gone", points: 500, active: false);
        var handler = new GetPublicOverviewHandler(_fixture.UserSummaryProjection, _fixture.RankThresholdProvider);

        var result = await handler.Handle(new GetPublicOverviewQuery());

        Assert.Equal(2, result.Value!.ActiveMemberCount);
        Assert.Equal(100, result.Value.TotalClanPoints);
        Assert.Equal(2, result.Value.RankDistribution.Sum(r => r.Count));
        Assert.Equal(1, result.Value.RankDistribution.Single(r => r.Rank == Rank.Bronze).Count);
    }

    // -- Ranks --

    [Fact]
    public async Task Ranks_ReturnsLadderAscendingByPoints()
    {
        var handler = new GetPublicRanksHandler(_fixture.RankThresholdProvider);

        var result = await handler.Handle(new GetPublicRanksQuery());

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(14, result.Value!.Ranks.Count);
        Assert.Equal(Rank.Bronze, result.Value.Ranks[0].Rank);
        Assert.Equal(Rank.Zenyte, result.Value.Ranks[^1].Rank);
        Assert.Equal(result.Value.Ranks.OrderBy(r => r.PointsRequired).ToList(), result.Value.Ranks);
    }

    // -- Leaderboard --

    [Fact]
    public async Task Leaderboard_OrdersByPointsThenRsn_AndExcludesInactiveAndRsnLess()
    {
        await CreateMemberAsync(101, "Zed", points: 50);
        await CreateMemberAsync(102, "Amy", points: 50);
        await CreateMemberAsync(103, "Top", points: 200);
        await CreateMemberAsync(104, null, points: 999);
        await CreateMemberAsync(105, "Gone", points: 999, active: false);
        var handler = new GetPublicLeaderboardHandler(_fixture.UserSummaryProjection);

        var result = await handler.Handle(new GetPublicLeaderboardQuery());

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(["Top", "Amy", "Zed"], result.Value!.Entries.Select(e => e.RunescapeUsername));
        Assert.Equal([1, 2, 3], result.Value.Entries.Select(e => e.Position));
        Assert.Equal(200, result.Value.Entries[0].ClanPoints);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(2, 2)]
    [InlineData(1000, 3)]
    public async Task Leaderboard_ClampsTop(int top, int expectedCount)
    {
        for (ulong i = 1; i <= 3; i++)
            await CreateMemberAsync(100 + i, $"Player{i}", points: (int)i);
        var handler = new GetPublicLeaderboardHandler(_fixture.UserSummaryProjection);

        var result = await handler.Handle(new GetPublicLeaderboardQuery { Top = top });

        Assert.Equal(expectedCount, result.Value!.Entries.Count);
    }
}
