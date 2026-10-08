using Discord.Interactions;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Formatting;

namespace RngHelpdesk.DiscordBot.Interactions;

/// <summary>The /points group. Only self-service `mine` lives here; admin subcommands are added later.</summary>
[Group("points", "Clan points.")]
public sealed class PointsModule(RngApiClient api) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("mine", "Show your last 10 point changes.")]
    public async Task MineAsync()
    {
        await DeferAsync(ephemeral: true);
        var history = await api.GetMyPointHistoryAsync(Context.User.Id);
        await ModifyOriginalResponseAsync(m => m.Embed = Embeds.PointHistory(history, "Your point history"));
    }
}
