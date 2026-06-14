using RngHelpdesk.Contracts.Common.Ranks;

namespace RngHelpdesk.Infrastructure.Persistence.Points;

public sealed class InMemoryRankThresholdProvider : IRankThresholdProvider
{
    public IReadOnlyList<RankThreshold> GetThresholds()
    {
        return
        [
            new(Rank.Bronze, 0),
            new(Rank.Iron, 10),
            new(Rank.Steel, 50),
            new(Rank.Mithril, 100),
            new(Rank.Adamant, 175),
            new(Rank.Rune, 265),
            new(Rank.Dragon, 375),
            new(Rank.Sapphire, 550),
            new(Rank.Emerald, 750),
            new(Rank.Ruby, 1000),
            new(Rank.Diamond, 2000),
            new(Rank.Dragonstone, 3000),
            new(Rank.Onyx, 4000),
            new(Rank.Zenyte, 5000)

            // Admins don't require a specific threshold and the ranks are overriden when an Admin rank exists.
        ];
    }
}
