using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using RngHelpdesk.DiscordBot.Interactions.Confirm;

namespace RngHelpdesk.DiscordBot.Interactions;

[Group("role", "Change a member's helpdesk role (admins only)")]
public sealed class RoleModule(ILogger<RoleModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("promote", "Promote a member to Administrator (asks for confirmation).")]
    public Task PromoteAsync([Summary(description: "The member")] IUser user)
        => PromptAsync(user, ConfirmAction.Promote, $"Promote {user.Mention} to Administrator?");

    [SlashCommand("demote", "Demote a member to Member, removing admin rights (asks for confirmation).")]
    public Task DemoteAsync([Summary(description: "The member")] IUser user)
        => PromptAsync(user, ConfirmAction.Demote, $"Demote {user.Mention} to Member? This removes their admin rights.");

    private async Task PromptAsync(IUser user, ConfirmAction action, string prompt)
    {
        if (user.Id == Context.User.Id)
        {
            await RespondAsync(SelfActionGuard.Message(action), ephemeral: true);
            return;
        }

        await ConfirmFlow.PromptAsync((SocketSlashCommand)Context.Interaction, prompt, action, user.Id, logger);
    }
}
