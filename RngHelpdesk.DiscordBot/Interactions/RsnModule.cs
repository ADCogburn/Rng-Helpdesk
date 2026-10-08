using Discord;
using Discord.Interactions;
using RngHelpdesk.DiscordBot.Formatting;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Interactions.Autocomplete;

namespace RngHelpdesk.DiscordBot.Interactions;

[Group("rsn", "Manage a member's RuneScape names (admins only)")]
public sealed class RsnModule(RngApiClient api) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("list", "Show a member's current and previous RuneScape names.")]
    public async Task ListAsync([Summary(description: "The member")] IUser user)
    {
        await DeferAsync(ephemeral: true);
        var actor = Context.User.Id;
        var current = await api.GetRunescapeAccountsAsync(actor, user.Id);
        var previous = await api.GetPreviousRunescapeAccountsAsync(actor, user.Id);
        await FollowupAsync(embed: Embeds.RunescapeAccounts(current, previous, $"RuneScape names — {user.Username}"), ephemeral: true);
    }

    [SlashCommand("link", "Link a RuneScape name to a member.")]
    public async Task LinkAsync([Summary(description: "The member")] IUser user, [Summary(description: "RuneScape name")] string rsn)
    {
        rsn = rsn.Trim();
        if (!RsnRules.IsValid(rsn))
        {
            await RespondAsync(RsnRules.InvalidMessage, ephemeral: true);
            return;
        }

        await DeferAsync(ephemeral: true);
        await api.LinkRunescapeAccountAsync(Context.User.Id, user.Id, rsn);
        await FollowupAsync($"Linked **{rsn}** to {user.Mention}.", ephemeral: true);
    }

    [SlashCommand("delink", "Remove a RuneScape name from a member.")]
    public async Task DelinkAsync(
        [Summary(description: "The member")] IUser user,
        [Summary(description: "RuneScape name to remove"), Autocomplete(typeof(CurrentRsnAutocompleteHandler))] string rsn)
    {
        rsn = rsn.Trim();
        if (!RsnRules.IsValid(rsn))
        {
            await RespondAsync(RsnRules.InvalidMessage, ephemeral: true);
            return;
        }

        await DeferAsync(ephemeral: true);
        await api.DelinkRunescapeAccountAsync(Context.User.Id, user.Id, rsn);
        await FollowupAsync($"Removed **{rsn}** from {user.Mention}.", ephemeral: true);
    }

    [SlashCommand("rename", "Rename one of a member's RuneScape names.")]
    public async Task RenameAsync(
        [Summary(description: "The member")] IUser user,
        [Summary(description: "Current RuneScape name"), Autocomplete(typeof(CurrentRsnAutocompleteHandler))] string old,
        [Summary(description: "New RuneScape name")] string @new)
    {
        old = old.Trim();
        @new = @new.Trim();
        if (!RsnRules.IsValid(old) || !RsnRules.IsValid(@new))
        {
            await RespondAsync(RsnRules.InvalidMessage, ephemeral: true);
            return;
        }

        await DeferAsync(ephemeral: true);
        await api.RenameRunescapeAccountAsync(Context.User.Id, user.Id, old, @new);
        await FollowupAsync($"Renamed **{old}** to **{@new}** for {user.Mention}.", ephemeral: true);
    }

    [SlashCommand("history", "Show a member's RuneScape name change history.")]
    public async Task HistoryAsync([Summary(description: "The member")] IUser user)
    {
        await DeferAsync(ephemeral: true);
        var history = await api.GetRunescapeAccountHistoryAsync(Context.User.Id, user.Id);
        await FollowupAsync(embed: Embeds.RunescapeAccountHistory(history, $"RuneScape name history — {user.Username}"), ephemeral: true);
    }
}
