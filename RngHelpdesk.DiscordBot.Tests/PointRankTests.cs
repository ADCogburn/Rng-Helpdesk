using RngHelpdesk.DiscordBot.Api.Models;
using RngHelpdesk.DiscordBot.Interactions;

namespace RngHelpdesk.DiscordBot.Tests;

public class PointRankTests
{
    [Fact]
    public void Offers_exactly_the_fourteen_point_based_ranks()
    {
        var mapped = Enum.GetValues<PointRank>().Select(p => (Rank)(int)p).ToList();

        Assert.Equal(14, mapped.Count);
        Assert.All(mapped, r => Assert.True(r <= Rank.Zenyte));
        Assert.Equal(Enum.GetValues<Rank>().Where(r => r <= Rank.Zenyte), mapped);
    }
}
