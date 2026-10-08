using Microsoft.Extensions.Logging;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Api.Models;

namespace RngHelpdesk.DiscordBot.Interactions;

/// <summary>
/// Fetches the rank ladder for embeds that show progress. The ladder is nice-to-have: a failed fetch
/// is logged and returns null so the command still replies without the progress field.
/// </summary>
public static class RankLadderLookup
{
    public static async Task<IReadOnlyList<RankThresholdDto>?> TryGetAsync(RngApiClient api, ILogger logger)
    {
        try
        {
            return (await api.GetRanksAsync()).Ranks;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not load rank ladder; replying without progress.");
            return null;
        }
    }
}
