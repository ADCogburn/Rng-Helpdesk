using Discord.Interactions;
using Microsoft.Extensions.Logging;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Api.Models;
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
        var thresholds = await TryGetThresholdsAsync();

        await ModifyOriginalResponseAsync(m => m.Embed = Embeds.Profile(user, thresholds, "Your profile"));
    }

    /// <summary>The progress field is nice-to-have; a failed ladder fetch must not fail the whole command.</summary>
    private async Task<IReadOnlyList<RankThresholdDto>?> TryGetThresholdsAsync()
    {
        try
        {
            return (await api.GetRanksAsync()).Ranks;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not load rank ladder for /profile; showing profile without progress.");
            return null;
        }
    }
}
