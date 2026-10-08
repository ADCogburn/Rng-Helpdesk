using Discord;
using Discord.Interactions;
using RngHelpdesk.DiscordBot.Api;

namespace RngHelpdesk.DiscordBot.Interactions.Autocomplete;

/// <summary>
/// Suggests the target user's current RSNs. Reads the already-entered <c>user</c> option from the
/// interaction, calls the API as the invoking user, and returns no suggestions on any failure.
/// </summary>
public sealed class CurrentRsnAutocompleteHandler(RngApiClient api, ILogger<CurrentRsnAutocompleteHandler> logger) : AutocompleteHandler
{
    public const string UserOptionName = "user";

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        try
        {
            // Discord.Net flattens group/subcommand nesting, so options are a flat list here.
            var data = autocompleteInteraction.Data;
            var userOption = data.Options.FirstOrDefault(o => o.Name == UserOptionName);

            if (!ulong.TryParse(userOption?.Value?.ToString(), out var targetId))
                return AutocompletionResult.FromSuccess();

            var accounts = await api.GetRunescapeAccountsAsync(context.User.Id, targetId);
            var names = RsnRules.Suggest(accounts.Accounts.Select(a => a.Username), data.Current.Value?.ToString());

            return AutocompletionResult.FromSuccess(names.Select(n => new AutocompleteResult(n, n)));
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "RSN autocomplete failed for user {UserId}.", context.User.Id);
            return AutocompletionResult.FromSuccess();
        }
    }
}
