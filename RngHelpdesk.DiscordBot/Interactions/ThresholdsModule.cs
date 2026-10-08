using Discord.Interactions;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Api.Models;
using RngHelpdesk.DiscordBot.Formatting;

namespace RngHelpdesk.DiscordBot.Interactions;

/// <summary>
/// Slash-command choice for the rank option: only the 14 point-based ranks. Values are the
/// <see cref="Rank"/> ordinals, so a choice maps back with <c>(Rank)value</c>.
/// </summary>
public enum PointRank
{
    Bronze = (int)Rank.Bronze,
    Iron = (int)Rank.Iron,
    Steel = (int)Rank.Steel,
    Mithril = (int)Rank.Mithril,
    Adamant = (int)Rank.Adamant,
    Rune = (int)Rank.Rune,
    Dragon = (int)Rank.Dragon,
    Sapphire = (int)Rank.Sapphire,
    Emerald = (int)Rank.Emerald,
    Ruby = (int)Rank.Ruby,
    Diamond = (int)Rank.Diamond,
    Dragonstone = (int)Rank.Dragonstone,
    Onyx = (int)Rank.Onyx,
    Zenyte = (int)Rank.Zenyte,
}

/// <summary>Admin-only (the API enforces AdminPlus). Replies are ephemeral.</summary>
[Group("thresholds", "View or change the points required for each rank.")]
public sealed class ThresholdsModule(RngApiClient api) : InteractionModuleBase<SocketInteractionContext>
{
    /// <summary>Rank resolution reads thresholds once at startup, so edits only take effect after a restart.</summary>
    private const string RestartReminder = "Rank resolution picks this up after an API restart.";

    [SlashCommand("view", "Show the current points required for each rank.")]
    public async Task ViewAsync()
    {
        await DeferAsync(ephemeral: true);
        var thresholds = await api.GetRankThresholdsAsync(Context.User.Id);
        await ModifyOriginalResponseAsync(m => m.Embed = Embeds.Ladder(thresholds.Thresholds, "Rank thresholds"));
    }

    [SlashCommand("set", "Change the points required for a rank.")]
    public async Task SetAsync(
        [Summary("rank", "The point-based rank to change.")] PointRank rank,
        [Summary("points", "Points required to reach this rank.")]
        [MinValue(0)]
        int points)
    {
        await DeferAsync(ephemeral: true);
        await api.SetRankThresholdAsync(Context.User.Id, (Rank)(int)rank, points);

        var thresholds = await api.GetRankThresholdsAsync(Context.User.Id);
        await ModifyOriginalResponseAsync(m =>
        {
            m.Content = RestartReminder;
            m.Embed = Embeds.Ladder(thresholds.Thresholds, $"Updated: {rank} now requires {points:N0} pts");
        });
    }
}
