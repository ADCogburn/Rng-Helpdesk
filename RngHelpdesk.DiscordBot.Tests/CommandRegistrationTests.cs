using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Interactions.Autocomplete;
using RngHelpdesk.DiscordBot.Interactions.Confirm;

namespace RngHelpdesk.DiscordBot.Tests;

/// <summary>
/// Builds the real InteractionService over the bot assembly without logging in and checks the command
/// tree that RegisterCommandsToGuildAsync would send, so shape mistakes fail in CI rather than at startup.
/// </summary>
public class CommandRegistrationTests : IAsyncLifetime
{
    private static readonly Regex NameRule = new("^[-_a-z0-9]{1,32}$");

    private InteractionService _interactions = null!;
    private ServiceProvider _services = null!;

    public async Task InitializeAsync()
    {
        var handler = new FakeHandler(_ => FakeHandler.Empty(HttpStatusCode.NotFound));
        var http = handler.CreateClient();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new DiscordSocketClient());
        services.AddSingleton(new ApiTokenService(http, "key"));
        services.AddSingleton(sp => new RngApiClient(http, sp.GetRequiredService<ApiTokenService>()));
        _services = services.BuildServiceProvider();

        _interactions = new InteractionService(new DiscordRestClient());
        await _interactions.AddModulesAsync(typeof(DiscordBotService).Assembly, _services);
    }

    public Task DisposeAsync()
    {
        _interactions.Dispose();
        return _services.DisposeAsync().AsTask();
    }

    private static string PathOf(SlashCommandInfo command)
        => command.Module.IsSlashGroup ? $"{command.Module.SlashGroupName} {command.Name}" : command.Name;

    [Fact]
    public void Command_set_is_exactly_the_planned_one()
    {
        var expected = new[]
        {
            "clan", "ranks", "leaderboard", "profile", "ping",
            "member info", "member find", "member add", "member deactivate", "member reactivate", "member lifecycle",
            "role promote", "role demote",
            "points mine", "points add", "points remove", "points history",
            "rsn list", "rsn link", "rsn delink", "rsn rename", "rsn history",
            "thresholds view", "thresholds set",
        };

        Assert.Equal(expected.OrderBy(x => x), _interactions.SlashCommands.Select(PathOf).OrderBy(x => x));
    }

    [Fact]
    public void Names_and_descriptions_follow_discord_rules()
    {
        foreach (var command in _interactions.SlashCommands)
        {
            Assert.Matches(NameRule, command.Name);
            Assert.InRange(command.Description.Length, 1, 100);

            foreach (var p in command.Parameters)
            {
                Assert.Matches(NameRule, p.Name);
                Assert.InRange(p.Description.Length, 1, 100);
            }
        }

        foreach (var module in _interactions.Modules.Where(m => m.IsSlashGroup))
        {
            Assert.Matches(NameRule, module.SlashGroupName);
            Assert.InRange(module.Description.Length, 1, 100);
        }
    }

    [Fact]
    public void Options_are_within_limits_and_required_come_first()
    {
        foreach (var command in _interactions.SlashCommands)
        {
            Assert.True(command.Parameters.Count <= 25, $"{PathOf(command)} has too many options");

            var seenOptional = false;
            foreach (var p in command.Parameters)
            {
                if (!p.IsRequired)
                    seenOptional = true;
                else
                    Assert.False(seenOptional, $"{PathOf(command)}: required option '{p.Name}' follows an optional one");
            }
        }
    }

    [Fact]
    public void Component_handlers_are_registered()
    {
        var names = _interactions.ComponentCommands.Select(c => c.Name).ToList();

        Assert.Contains(ConfirmCustomId.ConfirmPattern, names);
        Assert.Contains(ConfirmCustomId.CancelPattern, names);
    }

    [Fact]
    public void Rsn_autocomplete_is_attached_to_delink_rsn_and_rename_old()
    {
        var rsn = _interactions.SlashCommands.Where(c => c.Module.SlashGroupName == "rsn").ToDictionary(c => c.Name);

        Assert.Equal(typeof(CurrentRsnAutocompleteHandler), AutocompleteTypeOf(rsn["delink"], "rsn"));
        Assert.Equal(typeof(CurrentRsnAutocompleteHandler), AutocompleteTypeOf(rsn["rename"], "old"));
        Assert.Equal("user", CurrentRsnAutocompleteHandler.UserOptionName);
        Assert.Contains(rsn["delink"].Parameters, p => p.Name == CurrentRsnAutocompleteHandler.UserOptionName);
        Assert.Contains(rsn["rename"].Parameters, p => p.Name == CurrentRsnAutocompleteHandler.UserOptionName);
    }

    private static Type? AutocompleteTypeOf(SlashCommandInfo command, string option)
    {
        var parameter = command.Parameters.Single(p => p.Name == option);
        Assert.True(parameter.IsAutocomplete, $"{option} is not autocomplete");
        return parameter.AutocompleteHandler?.GetType();
    }

    [Fact]
    public void Commands_convert_to_application_command_properties()
    {
        // RegisterCommandsToGuildAsync uses an internal converter; call it via reflection so the exact
        // payload shape is validated offline.
        var assembly = typeof(InteractionService).Assembly;
        var util = assembly.GetType("Discord.Interactions.ApplicationCommandRestUtil", throwOnError: true)!;
        var method = util.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.Name == "ToApplicationCommandProps" && m.GetParameters().Length >= 1
                && m.GetParameters()[0].ParameterType == typeof(ModuleInfo));

        Assert.NotNull(method);

        var topLevel = _interactions.Modules.Where(m => !m.IsSubModule && (m.IsSlashGroup || m.SlashCommands.Count > 0)).ToList();
        var props = new List<ApplicationCommandProperties>();

        foreach (var module in topLevel)
        {
            var args = method!.GetParameters().Select(p => p.HasDefaultValue ? p.DefaultValue : null).ToArray();
            args[0] = module;
            var result = method.Invoke(null, args);
            if (result is IEnumerable<ApplicationCommandProperties> many)
                props.AddRange(many);
            else if (result is ApplicationCommandProperties one)
                props.Add(one);
        }

        var names = props.Select(p => p.Name.GetValueOrDefault()).ToList();
        Assert.All(names, n => Assert.Matches(NameRule, n));
        Assert.Contains("member", names);
        Assert.Contains("ping", names);
        Assert.Contains("clan", names);
        Assert.Equal(names.Count, names.Distinct().Count()); // no top-level name collisions
        Assert.True(names.Count <= 100);
    }
}
