using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using RngHelpdesk.DiscordBot.Api;

namespace RngHelpdesk.DiscordBot.Interactions.Confirm;

/// <summary>Handles the Confirm/Cancel buttons shown by destructive /member and /role commands.</summary>
public sealed class ConfirmModule(RngApiClient api, ILogger<ConfirmModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction(ConfirmCustomId.ConfirmPattern)]
    public async Task ConfirmAsync(string action, string target, string invoker)
    {
        var component = (SocketMessageComponent)Context.Interaction;

        if (!ConfirmCustomId.TryParse(action, target, invoker, out var parsed, out var targetId, out var invokerId))
        {
            await RespondAsync("That confirmation is no longer valid.", ephemeral: true);
            return;
        }

        if (Context.User.Id != invokerId)
        {
            await RespondAsync("Only the admin who ran the command can use these buttons.", ephemeral: true);
            return;
        }

        if (!ConfirmFlow.TryClaim(component.Message.Id))
        {
            await component.UpdateAsync(m =>
            {
                m.Content = "*This confirmation has expired — nothing was changed.*";
                m.Components = new ComponentBuilder().Build();
            });
            return;
        }

        await component.UpdateAsync(m =>
        {
            m.Content = "Working…";
            m.Components = new ComponentBuilder().Build();
        });

        string result;
        try
        {
            result = await ExecuteAsync(parsed, invokerId, targetId);
        }
        catch (Exception ex)
        {
            var reply = ErrorReplies.Describe(ex);
            if (reply.Unexpected)
                logger.LogError(ex, "Confirmed action {Action} failed for user {UserId}.", parsed, invokerId);
            result = reply.Message;
        }

        await component.ModifyOriginalResponseAsync(m => m.Content = result);
    }

    [ComponentInteraction(ConfirmCustomId.CancelPattern)]
    public async Task CancelAsync(string invoker)
    {
        var component = (SocketMessageComponent)Context.Interaction;

        if (!ulong.TryParse(invoker, out var invokerId) || Context.User.Id != invokerId)
        {
            await RespondAsync("Only the admin who ran the command can use these buttons.", ephemeral: true);
            return;
        }

        ConfirmFlow.TryClaim(component.Message.Id);
        await component.UpdateAsync(m =>
        {
            m.Content = "Cancelled — nothing was changed.";
            m.Components = new ComponentBuilder().Build();
        });
    }

    private async Task<string> ExecuteAsync(ConfirmAction action, ulong actor, ulong target)
    {
        // Mirrors the web UI: no self-service deactivate/promote/demote (re-checked here, not just on the command).
        if (actor == target)
            return SelfActionGuard.Message(action);

        switch (action)
        {
            case ConfirmAction.Deactivate:
                await api.DeactivateAsync(actor, target);
                return $"<@{target}> has been deactivated.";
            case ConfirmAction.Promote:
                await api.PromoteAsync(actor, target);
                return $"<@{target}> has been promoted to Administrator.";
            case ConfirmAction.Demote:
                await api.DemoteAsync(actor, target);
                return $"<@{target}> has been demoted to Member.";
            default:
                throw new InvalidOperationException($"Unhandled confirm action {action}.");
        }
    }
}

public static class SelfActionGuard
{
    public static string Message(ConfirmAction action) => action switch
    {
        ConfirmAction.Deactivate => "You can't deactivate your own account.",
        ConfirmAction.Promote => "You can't promote yourself.",
        _ => "You can't demote yourself.",
    };
}
