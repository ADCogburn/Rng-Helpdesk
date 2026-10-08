using RngHelpdesk.DiscordBot.Api.Models;
using RngHelpdesk.DiscordBot.Formatting;

namespace RngHelpdesk.DiscordBot.Tests;

public class RankStyleTests
{
    [Theory]
    [InlineData(Rank.Bronze, 0xa8703au)]
    [InlineData(Rank.Iron, 0x8a8d91u)]
    [InlineData(Rank.Steel, 0xb4bcc6u)]
    [InlineData(Rank.Mithril, 0x5a64b8u)]
    [InlineData(Rank.Adamant, 0x4f8a5bu)]
    [InlineData(Rank.Rune, 0x4fb3c7u)]
    [InlineData(Rank.Dragon, 0xc0392bu)]
    [InlineData(Rank.Sapphire, 0x2f6fd6u)]
    [InlineData(Rank.Emerald, 0x2ea86bu)]
    [InlineData(Rank.Ruby, 0xc21f4au)]
    [InlineData(Rank.Diamond, 0xcfe8f5u)]
    [InlineData(Rank.Dragonstone, 0xa35bd6u)]
    [InlineData(Rank.Onyx, 0x2b2b2bu)]
    [InlineData(Rank.Zenyte, 0xf0a33au)]
    public void Point_ranks_use_ui_plan_colours(Rank rank, uint expected)
        => Assert.Equal(expected, RankStyle.ColorFor(rank).RawValue);

    [Theory]
    [InlineData(Rank.Administrator)]
    [InlineData(Rank.DeputyOwner)]
    [InlineData(Rank.Owner)]
    public void Role_based_ranks_are_gold(Rank rank)
        => Assert.Equal(0xd4a84bu, RankStyle.ColorFor(rank).RawValue);

    [Fact]
    public void Every_point_rank_has_a_distinct_emoji()
    {
        var emojis = Enum.GetValues<Rank>()
            .Where(RankStyle.IsPointBased)
            .Select(RankStyle.EmojiFor)
            .ToList();

        Assert.Equal(14, emojis.Count);
        Assert.Equal(14, emojis.Distinct().Count());
    }

    [Theory]
    [InlineData(Rank.Administrator)]
    [InlineData(Rank.DeputyOwner)]
    [InlineData(Rank.Owner)]
    public void Role_based_ranks_get_a_crown(Rank rank)
        => Assert.Equal("👑", RankStyle.EmojiFor(rank));

    [Fact]
    public void Point_based_is_bronze_through_zenyte()
    {
        Assert.True(RankStyle.IsPointBased(Rank.Bronze));
        Assert.True(RankStyle.IsPointBased(Rank.Zenyte));
        Assert.False(RankStyle.IsPointBased(Rank.Administrator));
        Assert.False(RankStyle.IsPointBased(Rank.Owner));
    }

    private static readonly IReadOnlyList<RankThresholdDto> Ladder =
    [
        new(Rank.Bronze, 0),
        new(Rank.Iron, 100),
        new(Rank.Steel, 300),
        new(Rank.Mithril, 600),
    ];

    [Fact]
    public void Progress_is_fraction_between_current_and_next_threshold()
    {
        var progress = RankStyle.Progress(150, Ladder);

        Assert.Equal(Rank.Steel, progress.Next);
        Assert.Equal(150, progress.PointsToNext);
        Assert.Equal(0.25, progress.Fraction, 6);
    }

    [Fact]
    public void Progress_at_exact_threshold_counts_as_reached()
    {
        var progress = RankStyle.Progress(100, Ladder);

        Assert.Equal(Rank.Steel, progress.Next);
        Assert.Equal(200, progress.PointsToNext);
        Assert.Equal(0.0, progress.Fraction, 6);
    }

    [Fact]
    public void Progress_at_zero_points_points_at_the_next_rank()
    {
        var progress = RankStyle.Progress(0, Ladder);

        Assert.Equal(Rank.Iron, progress.Next);
        Assert.Equal(100, progress.PointsToNext);
        Assert.Equal(0.0, progress.Fraction, 6);
    }

    [Fact]
    public void Progress_below_first_threshold_measures_from_zero()
    {
        var thresholds = new RankThresholdDto[] { new(Rank.Bronze, 50), new(Rank.Iron, 100) };

        var progress = RankStyle.Progress(25, thresholds);

        Assert.Equal(Rank.Bronze, progress.Next);
        Assert.Equal(25, progress.PointsToNext);
        Assert.Equal(0.5, progress.Fraction, 6);
    }

    [Fact]
    public void Progress_at_max_rank_has_no_next_rank()
    {
        var progress = RankStyle.Progress(600, Ladder);

        Assert.Null(progress.Next);
        Assert.Equal(0, progress.PointsToNext);
        Assert.Equal(1.0, progress.Fraction);
    }

    [Fact]
    public void Progress_far_beyond_max_rank_is_still_max()
        => Assert.Null(RankStyle.Progress(1_000_000, Ladder).Next);

    [Fact]
    public void Progress_ignores_role_based_thresholds()
    {
        var thresholds = new RankThresholdDto[]
        {
            new(Rank.Bronze, 0),
            new(Rank.Administrator, 1),
        };

        var progress = RankStyle.Progress(5, thresholds);

        Assert.Null(progress.Next);
        Assert.Equal(1.0, progress.Fraction);
    }

    [Fact]
    public void Progress_with_no_thresholds_is_max_rank()
        => Assert.Null(RankStyle.Progress(10, Array.Empty<RankThresholdDto>()).Next);

    [Fact]
    public void Progress_bar_fills_proportionally()
        => Assert.Equal("▰▰▰▱▱▱▱▱▱▱▱▱", RankStyle.ProgressBar(0.25));

    [Fact]
    public void Progress_bar_empty_and_full()
    {
        Assert.Equal("▱▱▱▱▱▱▱▱▱▱▱▱", RankStyle.ProgressBar(0));
        Assert.Equal("▰▰▰▰▰▰▰▰▰▰▰▰", RankStyle.ProgressBar(1));
    }

    [Fact]
    public void Progress_bar_clamps_out_of_range_and_nan()
    {
        Assert.Equal("▱▱▱▱", RankStyle.ProgressBar(-3, 4));
        Assert.Equal("▰▰▰▰", RankStyle.ProgressBar(7, 4));
        Assert.Equal("▱▱▱▱", RankStyle.ProgressBar(double.NaN, 4));
    }

    [Fact]
    public void Progress_bar_respects_custom_width()
        => Assert.Equal("▰▰▱▱", RankStyle.ProgressBar(0.5, 4));

    [Fact]
    public void Progress_bar_with_non_positive_width_is_empty()
        => Assert.Equal(string.Empty, RankStyle.ProgressBar(0.5, 0));
}
