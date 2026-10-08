using Discord;
using RngHelpdesk.DiscordBot.Api.Models;
using RngHelpdesk.DiscordBot.Formatting;

namespace RngHelpdesk.DiscordBot.Tests;

public class EmbedsTests
{
    private static readonly DateTimeOffset Created = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    private static readonly IReadOnlyList<RankThresholdDto> Thresholds =
    [
        new(Rank.Zenyte, 900),
        new(Rank.Bronze, 0),
        new(Rank.Iron, 100),
        new(Rank.Steel, 300),
    ];

    private static UserResponse User(
        long points = 150,
        Rank rank = Rank.Iron,
        AppRole role = AppRole.Member,
        bool active = true,
        params string[] rsns) => new(
            Id: 111222333444555666,
            AppRole: role,
            ClanPoints: points,
            Rank: rank,
            IsActive: active,
            DateCreated: Created,
            DiscordAccount: new DiscordAccountDto(111222333444555666, "alice"),
            RunescapeAccounts: rsns.Select(r => new RunescapeAccountDto(r)).ToList());

    private static string FieldValue(Embed embed, string name)
        => embed.Fields.Single(f => f.Name == name).Value;

    private static string Unix(DateTimeOffset value) => value.ToUnixTimeSeconds().ToString();

    [Fact]
    public void Overview_shows_counts_and_ranked_distribution()
    {
        var overview = new PublicOverviewResponse(
            ActiveMemberCount: 42,
            TotalClanPoints: 12345,
            RankDistribution: [new(Rank.Iron, 3), new(Rank.Bronze, 9)]);

        var embed = Embeds.Overview(overview);

        Assert.Equal("42", FieldValue(embed, "Active members"));
        Assert.Equal("12,345", FieldValue(embed, "Total clan points"));
        Assert.Equal("🟫 Bronze: **9**\n⬜ Iron: **3**", FieldValue(embed, "Rank distribution"));
    }

    [Fact]
    public void Ladder_lists_tiers_in_ascending_points_order()
    {
        var embed = Embeds.Ladder(Thresholds);

        Assert.Equal("Rank ladder", embed.Title);
        Assert.Equal(
            "🟫 **Bronze** — 0 pts\n⬜ **Iron** — 100 pts\n🩶 **Steel** — 300 pts\n🔶 **Zenyte** — 900 pts",
            embed.Description);
    }

    [Fact]
    public void Leaderboard_uses_medals_for_top_three_and_rank_colour_of_first()
    {
        var board = new PublicLeaderboardResponse(
        [
            new(4, "Delta", Rank.Bronze, 20),
            new(2, "Beta", Rank.Steel, 300),
            new(1, "Alpha", Rank.Zenyte, 950),
            new(3, "Gamma", Rank.Iron, 150),
        ]);

        var embed = Embeds.Leaderboard(board);

        Assert.Equal(
            "🥇 **Alpha** — 🔶 Zenyte · 950 pts\n" +
            "🥈 **Beta** — 🩶 Steel · 300 pts\n" +
            "🥉 **Gamma** — ⬜ Iron · 150 pts\n" +
            "`#4` **Delta** — 🟫 Bronze · 20 pts",
            embed.Description);
        Assert.Equal<uint?>(RankStyle.ColorFor(Rank.Zenyte).RawValue, embed.Color?.RawValue);
    }

    [Fact]
    public void Leaderboard_empty_says_so()
        => Assert.Equal("No ranked members yet.",
            Embeds.Leaderboard(new PublicLeaderboardResponse([])).Description);

    [Fact]
    public void Profile_shows_rank_points_and_progress_to_next_rank()
    {
        var embed = Embeds.Profile(User(points: 150, rank: Rank.Iron), Thresholds, "Your profile");

        Assert.Equal("Your profile", embed.Title);
        Assert.Equal<uint?>(RankStyle.ColorFor(Rank.Iron).RawValue, embed.Color?.RawValue);
        Assert.Equal("⬜ Iron", FieldValue(embed, "Rank"));
        Assert.Equal("150", FieldValue(embed, "Clan points"));
        Assert.Equal("Member", FieldValue(embed, "Role"));
        Assert.Equal("Active", FieldValue(embed, "Status"));
        Assert.Equal("`▰▰▰▱▱▱▱▱▱▱▱▱` 25% — 150 pts to 🩶 **Steel**", FieldValue(embed, "Progress"));
    }

    [Fact]
    public void Profile_at_max_rank_says_so()
    {
        var embed = Embeds.Profile(User(points: 950, rank: Rank.Zenyte), Thresholds, "Profile");

        Assert.Equal("Max rank reached", FieldValue(embed, "Progress"));
    }

    [Fact]
    public void Profile_without_thresholds_omits_progress()
    {
        var embed = Embeds.Profile(User(), null, "Profile");

        Assert.DoesNotContain(embed.Fields, f => f.Name == "Progress");
    }

    [Fact]
    public void Profile_for_role_based_rank_omits_progress_and_is_gold()
    {
        var embed = Embeds.Profile(
            User(points: 5000, rank: Rank.Administrator, role: AppRole.Administrator),
            Thresholds,
            "Profile");

        Assert.DoesNotContain(embed.Fields, f => f.Name == "Progress");
        Assert.Equal("👑 Administrator", FieldValue(embed, "Rank"));
        Assert.Equal("Administrator", FieldValue(embed, "Role"));
        Assert.Equal<uint?>(0xd4a84bu, embed.Color?.RawValue);
    }

    [Fact]
    public void Profile_lists_rsns_discord_and_join_date()
    {
        var embed = Embeds.Profile(User(rsns: ["Main", "Alt"]), Thresholds, "Profile");

        Assert.Equal("Main\nAlt", FieldValue(embed, "RSNs"));
        Assert.Equal("<@111222333444555666> · alice", FieldValue(embed, "Discord"));
        Assert.Equal($"<t:{Unix(Created)}:D>", FieldValue(embed, "Member since"));
    }

    [Fact]
    public void Profile_with_no_rsns_and_deactivated()
    {
        var embed = Embeds.Profile(User(active: false), Thresholds, "Profile");

        Assert.Equal("None linked", FieldValue(embed, "RSNs"));
        Assert.Equal("Deactivated", FieldValue(embed, "Status"));
    }

    [Fact]
    public void Profile_sanitizes_markdown_in_rsns()
        => Assert.Equal("\\*Star\\*", FieldValue(Embeds.Profile(User(rsns: ["*Star*"]), null, "Profile"), "RSNs"));

    [Fact]
    public void Profile_rsn_list_is_truncated_to_field_limit()
    {
        var rsns = Enumerable.Range(0, 200).Select(i => $"Player{i:000}").ToArray();

        var value = FieldValue(Embeds.Profile(User(rsns: rsns), null, "Profile"), "RSNs");

        Assert.True(value.Length <= 1024);
        Assert.Contains("more", value);
    }

    [Fact]
    public void PointHistory_shows_signed_deltas_reason_and_rank_change()
    {
        var history = new PointHistoryResponse(
            UserId: 1,
            TotalEventCount: 5,
            Events:
            [
                new(50, "Raid", Created, Rank.Iron, Rank.Steel),
                new(-20, "Correction", Created, Rank.Steel, Rank.Steel),
            ]);

        var embed = Embeds.PointHistory(history, "Points");

        var timestamp = $"<t:{Unix(Created)}:R>";
        Assert.Equal(
            $"`+50` Raid · {timestamp} · ⬜ Iron → 🩶 Steel\n" +
            $"`-20` Correction · {timestamp}",
            embed.Description);
        Assert.Equal("Showing 2 of 5", embed.Footer?.Text);
    }

    [Fact]
    public void PointHistory_respects_max()
    {
        var events = Enumerable.Range(0, 15)
            .Select(i => new PointHistoryEvent(1, $"Event {i}", Created, null, null))
            .ToList();
        var history = new PointHistoryResponse(1, 15, events);

        var embed = Embeds.PointHistory(history, "Points", max: 10);

        Assert.Equal(10, embed.Description.Split('\n').Length);
        Assert.Equal("Showing 10 of 15", embed.Footer?.Text);
    }

    [Fact]
    public void PointHistory_empty_says_so()
        => Assert.Equal("No point changes yet.",
            Embeds.PointHistory(new PointHistoryResponse(1, 0, []), "Points").Description);

    [Fact]
    public void PointHistory_truncates_long_reasons()
    {
        var reason = new string('x', 500);
        var history = new PointHistoryResponse(1, 1, [new(5, reason, Created, null, null)]);

        var line = Embeds.PointHistory(history, "Points").Description;

        Assert.DoesNotContain(new string('x', 101), line);
        Assert.Contains("…", line);
    }

    [Fact]
    public void Lifecycle_lists_events_with_date_timestamps()
    {
        var lifecycle = new UserLifecycleResponse(1, [new("Created", Created)]);

        var embed = Embeds.Lifecycle(lifecycle, "Lifecycle");

        Assert.Equal($"<t:{Unix(Created)}:D> · Created", embed.Description);
    }

    [Fact]
    public void Lifecycle_empty_says_so()
        => Assert.Equal("No lifecycle events yet.",
            Embeds.Lifecycle(new UserLifecycleResponse(1, []), "Lifecycle").Description);

    [Fact]
    public void RunescapeAccounts_shows_current_and_previous_with_fallbacks()
    {
        var embed = Embeds.RunescapeAccounts(
            new RunescapeAccountsResponse([new("Main")]),
            new RunescapeAccountsResponse([]),
            "RSNs");

        Assert.Equal("RSNs", embed.Title);
        Assert.Equal("Main", FieldValue(embed, "Current"));
        Assert.Equal("None", FieldValue(embed, "Previous"));
    }

    [Fact]
    public void RunescapeAccounts_with_nothing_linked_shows_none_linked()
    {
        var embed = Embeds.RunescapeAccounts(
            new RunescapeAccountsResponse([]),
            new RunescapeAccountsResponse([]),
            "RSNs");

        Assert.Equal("None linked", FieldValue(embed, "Current"));
    }

    [Fact]
    public void RunescapeAccountHistory_renders_renames_as_arrows()
    {
        var history = new RunescapeAccountHistoryResponse(
        [
            new("Renamed", null, "Old Name", "New Name", Created),
            new("Linked", "Fresh", null, null, Created),
        ]);

        var embed = Embeds.RunescapeAccountHistory(history, "History");

        var timestamp = $"<t:{Unix(Created)}:R>";
        Assert.Equal(
            $"`Renamed` Old Name → New Name · {timestamp}\n" +
            $"`Linked` Fresh · {timestamp}",
            embed.Description);
    }

    [Fact]
    public void MemberCreated_spoilers_password_and_warns_it_is_shown_once()
    {
        var created = new CreateUserResponse(111222333444555666, "alice.rng", "s3cret-pass");

        var embed = Embeds.MemberCreated(created, "<@111222333444555666>");

        Assert.Contains("shown **once**", embed.Description);
        Assert.Equal("||`s3cret-pass`||", FieldValue(embed, "Temporary password"));
        Assert.Equal("`alice.rng`", FieldValue(embed, "Login username"));
        Assert.Equal("<@111222333444555666>", FieldValue(embed, "Discord"));
        Assert.Equal("111222333444555666", FieldValue(embed, "User id"));
    }

    [Fact]
    public void UsersList_shows_first_rsn_or_discord_name_and_mention()
    {
        var users = new[]
        {
            User(points: 150, rank: Rank.Iron, rsns: ["Main"]),
            User(points: 0, rank: Rank.Bronze, active: false),
        };

        var embed = Embeds.UsersList(users, "Matches");

        Assert.Equal(
            "⬜ **Main** — <@111222333444555666> · Iron · 150 pts\n" +
            "🟫 **alice** — <@111222333444555666> · Bronze · 0 pts · deactivated",
            embed.Description);
    }

    [Fact]
    public void UsersList_empty_says_so()
        => Assert.Equal("No matches.", Embeds.UsersList([], "Matches").Description);

    [Fact]
    public void Descriptions_are_truncated_to_discord_limit()
    {
        var lines = Enumerable.Range(0, 2000)
            .Select(i => new PointHistoryEvent(1, $"reason number {i}", Created, null, null))
            .ToList();

        var embed = Embeds.PointHistory(new PointHistoryResponse(1, 2000, lines), "Points", max: 2000);

        Assert.True(embed.Description.Length <= 4096);
        Assert.Matches(@"…and \d+ more$", embed.Description);
    }

    [Fact]
    public void Ladder_default_title_is_rank_ladder()
        => Assert.Equal("Rank ladder", Embeds.Ladder(Thresholds).Title);

    [Fact]
    public void Ladder_custom_title_is_used()
        => Assert.Equal("Thresholds", Embeds.Ladder(Thresholds, "Thresholds").Title);
}
