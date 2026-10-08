using Discord.Interactions;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Formatting;

namespace RngHelpdesk.DiscordBot.Interactions;

/// <summary>Anonymous clan stats. Replies are public (non-ephemeral) and need no Discord-side identity.</summary>
public sealed class PublicModule(RngApiClient api) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("clan", "Show the clan's active members, total points and rank distribution.")]
    public async Task ClanAsync()
    {
        await DeferAsync();
        var overview = await api.GetOverviewAsync();
        await ModifyOriginalResponseAsync(m => m.Embed = Embeds.Overview(overview));
    }

    [SlashCommand("ranks", "Show the rank ladder and the points required for each tier.")]
    public async Task RanksAsync()
    {
        await DeferAsync();
        var ranks = await api.GetRanksAsync();
        await ModifyOriginalResponseAsync(m => m.Embed = Embeds.Ladder(ranks.Ranks));
    }

    [SlashCommand("leaderboard", "Show the top clan members by points.")]
    public async Task LeaderboardAsync(
        [Summary("top", "How many members to show (1-25).")]
        [MinValue(1)]
        [MaxValue(25)]
        int top = 10)
    {
        await DeferAsync();
        var board = await api.GetLeaderboardAsync(top);
        await ModifyOriginalResponseAsync(m => m.Embed = Embeds.Leaderboard(board));
    }
}
