using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Formatting;

namespace RngHelpdesk.DiscordBot.Interactions;

/// <summary>
/// The /points group. `mine` is self-service and open to every registered member; `add`, `remove` and
/// `history` take a target member and are admin-only (the API enforces AdminPlus). All replies are ephemeral.
/// </summary>
[Group("points", "Clan points.")]
public sealed class PointsModule(RngApiClient api, ILogger<PointsModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
    private const int ReasonMaxLength = 200;

    [SlashCommand("mine", "Show your last 10 point changes.")]
    public async Task MineAsync()
    {
        await DeferAsync(ephemeral: true);
        var history = await api.GetMyPointHistoryAsync(Context.User.Id);
        await ModifyOriginalResponseAsync(m => m.Embed = Embeds.PointHistory(history, "Your point history"));
    }

    [SlashCommand("add", "Add clan points to a member.")]
    public Task AddAsync(
        [Summary("user", "The member to credit.")] IUser user,
        [Summary("amount", "Points to add.")]
        [MinValue(1)]
        int amount,
        [Summary("reason", "Why the points are being added (shown in their history).")]
        [MinLength(1)]
        [MaxLength(ReasonMaxLength)]
        string reason)
        => AdjustAsync(user, amount, reason, add: true);

    [SlashCommand("remove", "Remove clan points from a member.")]
    public Task RemoveAsync(
        [Summary("user", "The member to debit.")] IUser user,
        [Summary("amount", "Points to remove.")]
        [MinValue(1)]
        int amount,
        [Summary("reason", "Why the points are being removed (shown in their history).")]
        [MinLength(1)]
        [MaxLength(ReasonMaxLength)]
        string reason)
        => AdjustAsync(user, amount, reason, add: false);

    [SlashCommand("history", "Show a member's last 10 point changes.")]
    public async Task HistoryAsync([Summary("user", "The member to look up.")] IUser user)
    {
        await DeferAsync(ephemeral: true);
        var history = await api.GetPointHistoryAsync(Context.User.Id, user.Id);
        await ModifyOriginalResponseAsync(m => m.Embed = Embeds.PointHistory(history, $"Point history for {user.Username}"));
    }

    private async Task AdjustAsync(IUser user, int amount, string reason, bool add)
    {
        await DeferAsync(ephemeral: true);

        if (add)
            await api.AddPointsAsync(Context.User.Id, user.Id, amount, reason);
        else
            await api.RemovePointsAsync(Context.User.Id, user.Id, amount, reason);

        var updated = await api.GetUserAsync(Context.User.Id, user.Id);
        var thresholds = await RankLadderLookup.TryGetAsync(api, logger);

        var sign = add ? "+" : "-";
        var title = $"{sign}{amount:N0} points for {user.Username}";

        await ModifyOriginalResponseAsync(m => m.Embed = Embeds.Profile(updated, thresholds, title));
    }
}
