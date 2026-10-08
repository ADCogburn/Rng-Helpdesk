using Discord.Interactions;
using Microsoft.Extensions.Logging;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Formatting;

namespace RngHelpdesk.DiscordBot.Interactions;

/// <summary>The invoking member's own profile. Ephemeral: only the caller sees it.</summary>
public sealed class ProfileModule(RngApiClient api, ILogger<ProfileModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("profile", "Show your rank, points, progress to the next rank and linked RSNs.")]
    public async Task ProfileAsync()
    {
        await DeferAsync(ephemeral: true);

        var user = await api.GetMeAsync(Context.User.Id);
        var thresholds = await RankLadderLookup.TryGetAsync(api, logger);

        await ModifyOriginalResponseAsync(m => m.Embed = Embeds.Profile(user, thresholds, "Your profile"));
    }
}
