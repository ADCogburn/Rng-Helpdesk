using System.Globalization;
using System.Text;
using Discord;
using RngHelpdesk.DiscordBot.Api.Models;

namespace RngHelpdesk.DiscordBot.Formatting;

/// <summary>Pure DTO → embed builders. Every method returns a fresh <see cref="Embed"/>.</summary>
public static class Embeds
{
    private const int DescriptionLimit = 4096;
    private const int FieldValueLimit = 1024;
    private const int ReasonLimit = 100;

    private static readonly Color Brand = new(0xd4a84b);
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static Embed Overview(PublicOverviewResponse overview)
    {
        var distribution = overview.RankDistribution
            .OrderBy(r => r.Rank)
            .Select(r => $"{RankStyle.EmojiFor(r.Rank)} {DisplayName(r.Rank)}: **{r.Count}**");

        return new EmbedBuilder()
            .WithTitle("Clan overview")
            .WithColor(Brand)
            .AddField("Active members", overview.ActiveMemberCount.ToString(Invariant), true)
            .AddField("Total clan points", Points(overview.TotalClanPoints), true)
            .AddField("Rank distribution", Join(distribution, FieldValueLimit, "None"))
            .Build();
    }

    public static Embed Ladder(IReadOnlyList<RankThresholdDto> thresholds, string title = "Rank ladder")
    {
        var lines = thresholds
            .OrderBy(t => t.PointsRequired)
            .Select(t => $"{RankStyle.EmojiFor(t.Rank)} **{DisplayName(t.Rank)}** — {Points(t.PointsRequired)} pts");

        return new EmbedBuilder()
            .WithTitle(title)
            .WithColor(Brand)
            .WithDescription(Join(lines, DescriptionLimit, "No ranks configured."))
            .Build();
    }

    public static Embed Leaderboard(PublicLeaderboardResponse board)
    {
        var entries = board.Entries.OrderBy(e => e.Position).ToList();
        var lines = entries.Select(e =>
            $"{Medal(e.Position)} **{Discord.Format.Sanitize(e.RunescapeUsername)}** — " +
            $"{RankStyle.EmojiFor(e.Rank)} {DisplayName(e.Rank)} · {Points(e.ClanPoints)} pts");

        return new EmbedBuilder()
            .WithTitle("Leaderboard")
            .WithColor(entries.Count > 0 ? RankStyle.ColorFor(entries[0].Rank) : Brand)
            .WithDescription(Join(lines, DescriptionLimit, "No ranked members yet."))
            .Build();
    }

    /// <param name="thresholds">Null skips the progress field (e.g. when the ladder could not be fetched).</param>
    public static Embed Profile(UserResponse user, IReadOnlyList<RankThresholdDto>? thresholds, string title)
    {
        var builder = new EmbedBuilder()
            .WithTitle(title)
            .WithColor(RankStyle.ColorFor(user.Rank))
            .AddField("Rank", $"{RankStyle.EmojiFor(user.Rank)} {DisplayName(user.Rank)}", true)
            .AddField("Clan points", Points(user.ClanPoints), true)
            .AddField("Role", DisplayName(user.AppRole), true);

        if (thresholds is not null && RankStyle.IsPointBased(user.Rank))
        {
            builder.AddField("Progress", ProgressText(user.ClanPoints, thresholds), false);
        }

        return builder
            .AddField("Status", user.IsActive ? "Active" : "Deactivated", true)
            .AddField("Discord", $"<@{user.DiscordAccount.DiscordId}> · {Discord.Format.Sanitize(user.DiscordAccount.Username)}", true)
            .AddField("Member since", Timestamp(user.DateCreated, 'D'), true)
            .AddField("RSNs", Join(user.RunescapeAccounts.Select(a => Discord.Format.Sanitize(a.Username)), FieldValueLimit, "None linked"), false)
            .Build();
    }

    public static Embed PointHistory(PointHistoryResponse history, string title, int max = 10)
    {
        var shown = history.Events
            .Take(Math.Max(0, max))
            .Select(FormatPointEvent)
            .ToList();

        return new EmbedBuilder()
            .WithTitle(title)
            .WithColor(Brand)
            .WithDescription(Join(shown, DescriptionLimit, "No point changes yet."))
            .WithFooter($"Showing {shown.Count} of {history.TotalEventCount}")
            .Build();
    }

    public static Embed Lifecycle(UserLifecycleResponse lifecycle, string title)
    {
        var lines = lifecycle.History
            .Select(h => $"{Timestamp(h.OccurredAt, 'D')} · {Discord.Format.Sanitize(h.Action)}");

        return new EmbedBuilder()
            .WithTitle(title)
            .WithColor(Brand)
            .WithDescription(Join(lines, DescriptionLimit, "No lifecycle events yet."))
            .Build();
    }

    public static Embed RunescapeAccounts(RunescapeAccountsResponse current, RunescapeAccountsResponse previous, string title)
        => new EmbedBuilder()
            .WithTitle(title)
            .WithColor(Brand)
            .AddField("Current", Join(current.Accounts.Select(a => Discord.Format.Sanitize(a.Username)), FieldValueLimit, "None linked"))
            .AddField("Previous", Join(previous.Accounts.Select(a => Discord.Format.Sanitize(a.Username)), FieldValueLimit, "None"))
            .Build();

    public static Embed RunescapeAccountHistory(RunescapeAccountHistoryResponse history, string title)
    {
        var lines = history.History.Select(FormatRsnEvent);

        return new EmbedBuilder()
            .WithTitle(title)
            .WithColor(Brand)
            .WithDescription(Join(lines, DescriptionLimit, "No RSN history yet."))
            .Build();
    }

    /// <summary>Shown ephemerally to the admin only; the password is spoilered and warned as one-time.</summary>
    public static Embed MemberCreated(CreateUserResponse created, string discordMention)
        => new EmbedBuilder()
            .WithTitle("Member created")
            .WithColor(Brand)
            .WithDescription(
                "Share these credentials privately. The temporary password is shown **once** " +
                "and can't be retrieved again.")
            .AddField("Discord", discordMention, true)
            .AddField("User id", created.UserId.ToString(Invariant), true)
            .AddField("Login username", $"`{created.Username}`", false)
            .AddField("Temporary password", $"||`{created.TemporaryPassword}`||", false)
            .Build();

    public static Embed UsersList(IReadOnlyList<UserResponse> users, string title)
    {
        var lines = users.Select(u =>
        {
            var label = u.RunescapeAccounts.FirstOrDefault()?.Username ?? u.DiscordAccount.Username;
            var deactivated = u.IsActive ? string.Empty : " · deactivated";
            return $"{RankStyle.EmojiFor(u.Rank)} **{Discord.Format.Sanitize(label)}** — " +
                   $"<@{u.DiscordAccount.DiscordId}> · {DisplayName(u.Rank)} · {Points(u.ClanPoints)} pts{deactivated}";
        });

        return new EmbedBuilder()
            .WithTitle(title)
            .WithColor(Brand)
            .WithDescription(Join(lines, DescriptionLimit, "No matches."))
            .Build();
    }

    private static string ProgressText(long clanPoints, IReadOnlyList<RankThresholdDto> thresholds)
    {
        var progress = RankStyle.Progress(clanPoints, thresholds);
        if (progress.Next is not { } next)
        {
            return "Max rank reached";
        }

        var percent = (int)Math.Floor(progress.Fraction * 100);
        return $"`{RankStyle.ProgressBar(progress.Fraction)}` {percent}% — " +
               $"{Points(progress.PointsToNext)} pts to {RankStyle.EmojiFor(next)} **{DisplayName(next)}**";
    }

    private static string FormatPointEvent(PointHistoryEvent e)
    {
        var delta = e.Delta.ToString("+#,0;-#,0;0", Invariant);
        var reason = Discord.Format.Sanitize(Truncate(e.Reason, ReasonLimit));
        var line = $"`{delta}` {reason} · {Timestamp(e.OccurredAt, 'R')}";

        if (e.RankBefore is { } before && e.RankAfter is { } after && before != after)
        {
            line += $" · {RankStyle.EmojiFor(before)} {DisplayName(before)} → {RankStyle.EmojiFor(after)} {DisplayName(after)}";
        }

        return line;
    }

    private static string FormatRsnEvent(RunescapeAccountHistoryItem item)
    {
        var detail = item.OldUsername is not null && item.NewUsername is not null
            ? $"{Discord.Format.Sanitize(item.OldUsername)} → {Discord.Format.Sanitize(item.NewUsername)}"
            : Discord.Format.Sanitize(item.Username ?? "—");

        return $"`{item.ChangeType}` {detail} · {Timestamp(item.OccurredAt, 'R')}";
    }

    private static string Medal(int position) => position switch
    {
        1 => "🥇",
        2 => "🥈",
        3 => "🥉",
        _ => $"`#{position}`",
    };

    private static string Points(long points) => points.ToString("N0", Invariant);

    private static string Timestamp(DateTimeOffset value, char style)
        => $"<t:{value.ToUnixTimeSeconds()}:{style}>";

    private static string DisplayName(Rank rank) => rank switch
    {
        Rank.DeputyOwner => "Deputy Owner",
        _ => rank.ToString(),
    };

    private static string DisplayName(AppRole role) => role switch
    {
        AppRole.SuperAdministrator => "Super Administrator",
        _ => role.ToString(),
    };

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..(max - 1)] + "…";

    /// <summary>
    /// Joins <paramref name="items"/> with newlines, dropping trailing items with a "…and N more" tail
    /// so the result never exceeds <paramref name="limit"/>. Returns <paramref name="empty"/> when there are no items.
    /// </summary>
    private static string Join(IEnumerable<string> items, int limit, string empty)
    {
        var lines = items.ToList();
        if (lines.Count == 0)
        {
            return empty;
        }

        var full = string.Join("\n", lines);
        if (full.Length <= limit)
        {
            return full;
        }

        var sb = new StringBuilder();
        var included = 0;
        for (var i = 0; i < lines.Count; i++)
        {
            var candidate = (included == 0 ? string.Empty : "\n") + lines[i];
            var omitted = lines.Count - (i + 1);
            var suffix = omitted > 0 ? $"\n…and {omitted} more" : string.Empty;
            if (sb.Length + candidate.Length + suffix.Length > limit)
            {
                break;
            }

            sb.Append(candidate);
            included++;
        }

        var tail = $"…and {lines.Count - included} more";
        return included == 0 ? tail : $"{sb}\n{tail}";
    }
}
