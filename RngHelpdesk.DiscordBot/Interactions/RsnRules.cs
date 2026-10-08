using System.Text.RegularExpressions;

namespace RngHelpdesk.DiscordBot.Interactions;

/// <summary>Pure RSN helpers: client-side format check (mirrors the API rule) and autocomplete filtering.</summary>
public static partial class RsnRules
{
    public const int MaxAutocompleteChoices = 25;

    public const string InvalidMessage =
        "That isn't a valid RuneScape name: use 1–12 letters, numbers, spaces or hyphens, with no leading or trailing space.";

    [GeneratedRegex(@"^(?! )[A-Za-z0-9 -]{1,12}(?<! )\z")]
    private static partial Regex Pattern();

    public static bool IsValid(string? rsn) => rsn is not null && Pattern().IsMatch(rsn);

    /// <summary>Case-insensitive prefix filter over <paramref name="names"/>, distinct, capped at 25.</summary>
    public static IReadOnlyList<string> Suggest(IEnumerable<string> names, string? typed)
    {
        var prefix = typed?.Trim() ?? "";
        return names
            .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxAutocompleteChoices)
            .ToList();
    }
}
