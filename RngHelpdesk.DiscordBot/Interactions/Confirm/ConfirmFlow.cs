using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;

namespace RngHelpdesk.DiscordBot.Interactions.Confirm;

/// <summary>
/// Sends an ephemeral confirm/cancel prompt and tracks it so the 60 s timeout only edits the message if
/// the buttons were never pressed.
/// </summary>
public static class ConfirmFlow
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    private static readonly ConcurrentDictionary<ulong, byte> Pending = new();

    public static MessageComponent Buttons(ConfirmAction action, ulong targetId, ulong invokerId, bool disabled = false)
        => new ComponentBuilder()
            .WithButton("Confirm", ConfirmCustomId.Confirm(action, targetId, invokerId), ButtonStyle.Danger, disabled: disabled)
            .WithButton("Cancel", ConfirmCustomId.Cancel(invokerId), ButtonStyle.Secondary, disabled: disabled)
            .Build();

    public static async Task PromptAsync(SocketSlashCommand interaction, string prompt, ConfirmAction action, ulong targetId, ILogger logger)
    {
        await interaction.RespondAsync(prompt, components: Buttons(action, targetId, interaction.User.Id), ephemeral: true);
        var message = await interaction.GetOriginalResponseAsync();
        Pending[message.Id] = 0;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(Timeout);
                if (!Pending.TryRemove(message.Id, out _))
                    return;

                await interaction.ModifyOriginalResponseAsync(m =>
                {
                    m.Content = $"{prompt}\n\n*Timed out — nothing was changed.*";
                    m.Components = Buttons(action, targetId, interaction.User.Id, disabled: true);
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not disable timed-out confirm buttons.");
            }
        });
    }

    /// <summary>Marks the prompt as answered; false if it already timed out or was answered.</summary>
    public static bool TryClaim(ulong messageId) => Pending.TryRemove(messageId, out _);
}
