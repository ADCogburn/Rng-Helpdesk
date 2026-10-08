using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Formatting;
using RngHelpdesk.DiscordBot.Interactions.Confirm;

namespace RngHelpdesk.DiscordBot.Interactions;

[Group("member", "Manage clan members (admins only)")]
public sealed class MemberModule(RngApiClient api, ILogger<MemberModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("info", "Show a member's profile.")]
    public async Task InfoAsync([Summary(description: "The member")] IUser user)
    {
        await DeferAsync(ephemeral: true);
        var member = await api.GetUserAsync(Context.User.Id, user.Id);
        var ranks = await RankLadderLookup.TryGetAsync(api, logger);
        await FollowupAsync(embed: Embeds.Profile(member, ranks, "Member profile"), ephemeral: true);
    }

    [SlashCommand("find", "Find who owns (or previously used) a RuneScape name.")]
    public async Task FindAsync([Summary(description: "RuneScape name")] string rsn)
    {
        await DeferAsync(ephemeral: true);
        var actor = Context.User.Id;

        try
        {
            var owner = await api.GetUserByRsnAsync(actor, rsn);
            var ranks = await RankLadderLookup.TryGetAsync(api, logger);
            await FollowupAsync(embed: Embeds.Profile(owner, ranks, $"Current owner of {rsn}"), ephemeral: true);
            return;
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound && ex is not TokenExchangeException)
        {
            // Not a current RSN: fall through to the historical lookup.
        }

        IReadOnlyList<Api.Models.UserResponse> previous;
        try
        {
            previous = (await api.GetUsersByHistoricalRsnAsync(actor, rsn)).Users;
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound && ex is not TokenExchangeException)
        {
            previous = [];
        }

        if (previous.Count == 0)
        {
            await FollowupAsync($"No member has used the RuneScape name **{rsn}**.", ephemeral: true);
            return;
        }

        await FollowupAsync(embed: Embeds.UsersList(previous, $"Previously used by ({rsn})"), ephemeral: true);
    }

    [SlashCommand("add", "Register a Discord user as a clan member.")]
    public async Task AddAsync(
        [Summary(description: "The Discord user to register")] IUser user,
        [Summary(description: "RuneScape name")] string? rsn1 = null,
        [Summary(description: "Second RuneScape name")] string? rsn2 = null,
        [Summary(description: "Third RuneScape name")] string? rsn3 = null)
    {
        await DeferAsync(ephemeral: true);

        var rsns = new[] { rsn1, rsn2, rsn3 }
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var created = await api.CreateMemberAsync(Context.User.Id, user.Id, user.Username, rsns);
        logger.LogInformation("Admin {Admin} created member {Member}.", Context.User.Id, user.Id);
        await FollowupAsync(embed: Embeds.MemberCreated(created, user.Mention), ephemeral: true);
    }

    [SlashCommand("deactivate", "Deactivate a member (asks for confirmation).")]
    public async Task DeactivateAsync([Summary(description: "The member")] IUser user)
    {
        if (user.Id == Context.User.Id)
        {
            await RespondAsync(SelfActionGuard.Message(ConfirmAction.Deactivate), ephemeral: true);
            return;
        }

        await ConfirmFlow.PromptAsync(
            (SocketSlashCommand)Context.Interaction,
            $"Deactivate {user.Mention}? They will no longer be able to use the helpdesk.",
            ConfirmAction.Deactivate, user.Id, logger);
    }

    [SlashCommand("reactivate", "Reactivate a deactivated member.")]
    public async Task ReactivateAsync([Summary(description: "The member")] IUser user)
    {
        await DeferAsync(ephemeral: true);
        await api.ReactivateAsync(Context.User.Id, user.Id);
        await FollowupAsync($"{user.Mention} has been reactivated.", ephemeral: true);
    }

    [SlashCommand("lifecycle", "Show a member's account lifecycle timeline.")]
    public async Task LifecycleAsync([Summary(description: "The member")] IUser user)
    {
        await DeferAsync(ephemeral: true);
        var lifecycle = await api.GetLifecycleAsync(Context.User.Id, user.Id);
        await FollowupAsync(embed: Embeds.Lifecycle(lifecycle, $"Lifecycle — {user.Username}"), ephemeral: true);
    }
}
