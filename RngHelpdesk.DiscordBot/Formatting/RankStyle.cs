using Discord;
using RngHelpdesk.DiscordBot.Api.Models;

namespace RngHelpdesk.DiscordBot.Formatting;

/// <summary>Rank colours, emoji and point-progress maths shared by every embed.</summary>
public static class RankStyle
{
    private static readonly Color AdminTierColor = new(0xd4a84b);

    public static Color ColorFor(Rank rank) => rank switch
    {
        Rank.Bronze => new Color(0xa8703a),
        Rank.Iron => new Color(0x8a8d91),
        Rank.Steel => new Color(0xb4bcc6),
        Rank.Mithril => new Color(0x5a64b8),
        Rank.Adamant => new Color(0x4f8a5b),
        Rank.Rune => new Color(0x4fb3c7),
        Rank.Dragon => new Color(0xc0392b),
        Rank.Sapphire => new Color(0x2f6fd6),
        Rank.Emerald => new Color(0x2ea86b),
        Rank.Ruby => new Color(0xc21f4a),
        Rank.Diamond => new Color(0xcfe8f5),
        Rank.Dragonstone => new Color(0xa35bd6),
        Rank.Onyx => new Color(0x2b2b2b),
        Rank.Zenyte => new Color(0xf0a33a),
        Rank.Administrator or Rank.DeputyOwner or Rank.Owner => AdminTierColor,
        _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null),
    };

    public static string EmojiFor(Rank rank) => rank switch
    {
        Rank.Bronze => "🟫",
        Rank.Iron => "⬜",
        Rank.Steel => "🩶",
        Rank.Mithril => "🟦",
        Rank.Adamant => "🟩",
        Rank.Rune => "🩵",
        Rank.Dragon => "🐉",
        Rank.Sapphire => "🔹",
        Rank.Emerald => "🍀",
        Rank.Ruby => "🔺",
        Rank.Diamond => "💎",
        Rank.Dragonstone => "🔮",
        Rank.Onyx => "⚫",
        Rank.Zenyte => "🔶",
        Rank.Administrator or Rank.DeputyOwner or Rank.Owner => "👑",
        _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null),
    };

    /// <summary>True for the 14 points-based ranks (Bronze..Zenyte); false for role-based admin tiers.</summary>
    public static bool IsPointBased(Rank rank) => rank <= Rank.Zenyte;

    /// <summary>
    /// Progress from <paramref name="clanPoints"/> toward the next point-based rank in
    /// <paramref name="thresholds"/> (non-point-based entries are ignored). <see cref="RankProgress.Next"/>
    /// is null at the top of the ladder, and also when no thresholds are supplied.
    /// </summary>
    public static RankProgress Progress(long clanPoints, IReadOnlyList<RankThresholdDto> thresholds)
    {
        var ladder = thresholds
            .Where(t => IsPointBased(t.Rank))
            .OrderBy(t => t.PointsRequired)
            .ToList();

        var next = ladder.FirstOrDefault(t => t.PointsRequired > clanPoints);
        if (next is null)
        {
            return new RankProgress(null, 0, 1.0);
        }

        var floor = ladder.LastOrDefault(t => t.PointsRequired <= clanPoints)?.PointsRequired ?? 0;
        var span = next.PointsRequired - floor;
        var fraction = span <= 0 ? 0.0 : Math.Clamp((double)(clanPoints - floor) / span, 0.0, 1.0);

        return new RankProgress(next.Rank, next.PointsRequired - clanPoints, fraction);
    }

    /// <summary>Text bar such as "▰▰▰▰▱▱▱▱▱▱▱▱". Fraction is clamped to 0..1; NaN counts as 0.</summary>
    public static string ProgressBar(double fraction, int width = 12)
    {
        if (width <= 0)
        {
            return string.Empty;
        }

        var clamped = double.IsNaN(fraction) ? 0.0 : Math.Clamp(fraction, 0.0, 1.0);
        var filled = (int)Math.Round(clamped * width, MidpointRounding.AwayFromZero);
        return new string('▰', filled) + new string('▱', width - filled);
    }
}

/// <param name="Next">Next point-based rank, or null when at the top of the ladder.</param>
/// <param name="PointsToNext">Points still needed to reach <paramref name="Next"/> (0 at max rank).</param>
/// <param name="Fraction">Progress from the current rank's threshold toward <paramref name="Next"/>, 0..1.</param>
public sealed record RankProgress(Rank? Next, long PointsToNext, double Fraction);
