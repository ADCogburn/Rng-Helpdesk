using Discord.Interactions;
using Discord.WebSocket;

namespace RngHelpdesk.DiscordBot.Interactions;

/// <summary>Trivial command proving the gateway → interaction pipeline end to end.</summary>
public sealed class PingModule(DiscordSocketClient client) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Check that the helpdesk bot is alive.")]
    public Task PingAsync()
        => RespondAsync($"Pong! Gateway latency: {client.Latency} ms.", ephemeral: true);
}
