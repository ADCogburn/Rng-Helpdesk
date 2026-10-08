using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using RngHelpdesk.DiscordBot.Interactions;

namespace RngHelpdesk.DiscordBot;

/// <summary>
/// Owns the gateway connection: logs in, discovers interaction modules, registers slash commands to the
/// configured guild once ready, and routes interactions to <see cref="InteractionService"/>.
/// </summary>
public sealed class DiscordBotService(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    IOptions<BotOptions> options,
    ILogger<DiscordBotService> logger) : IHostedService
{
    private int _commandsRegistered;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        client.Log += LogAsync;
        interactions.Log += LogAsync;
        client.Ready += OnReadyAsync;
        client.InteractionCreated += OnInteractionCreatedAsync;
        interactions.SlashCommandExecuted += (_, context, result) => InteractionErrorHandler.HandleAsync(context, result, logger);
        interactions.ComponentCommandExecuted += (_, context, result) => InteractionErrorHandler.HandleAsync(context, result, logger);

        await interactions.AddModulesAsync(typeof(DiscordBotService).Assembly, services);

        await client.LoginAsync(TokenType.Bot, options.Value.Discord.BotToken);
        await client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        client.Ready -= OnReadyAsync;
        client.InteractionCreated -= OnInteractionCreatedAsync;
        await client.StopAsync();
        await client.LogoutAsync();
    }

    private async Task OnReadyAsync()
    {
        // Ready fires again after reconnects; the command set only needs registering once per process.
        if (Interlocked.Exchange(ref _commandsRegistered, 1) == 1)
            return;

        try
        {
            var registered = await interactions.RegisterCommandsToGuildAsync(options.Value.GuildIdValue);
            logger.LogInformation("Registered {Count} slash commands to guild {GuildId}.", registered.Count, options.Value.Discord.GuildId);
        }
        catch (Exception ex)
        {
            Interlocked.Exchange(ref _commandsRegistered, 0);
            logger.LogError(ex, "Failed to register slash commands to guild {GuildId}.", options.Value.Discord.GuildId);
        }
    }

    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        try
        {
            // Root provider, not a per-interaction scope: InteractionService's default RunMode.Async
            // runs the module after ExecuteCommandAsync returns, so a `using` scope here would already
            // be disposed. Every bot service is a singleton, so there is nothing scoped to resolve.
            var context = new SocketInteractionContext(client, interaction);
            await interactions.ExecuteCommandAsync(context, services);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error dispatching interaction {InteractionId}.", interaction.Id);
        }
    }

    private Task LogAsync(LogMessage message)
    {
        var level = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            _ => LogLevel.Trace,
        };

        logger.Log(level, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
        return Task.CompletedTask;
    }
}
