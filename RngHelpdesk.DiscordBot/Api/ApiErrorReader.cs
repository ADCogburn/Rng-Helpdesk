using System.Net;
using System.Text.Json;

namespace RngHelpdesk.DiscordBot.Api;

internal static class ApiErrorReader
{
    /// <summary>
    /// Extracts the API's message from an error body: a plain string (text or JSON string), or a
    /// ValidationProblemDetails whose field errors are flattened. Null when the body has nothing useful.
    /// </summary>
    public static string? ExtractMessage(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        var trimmed = body.Trim();

        if (trimmed.StartsWith('"'))
        {
            try { return JsonSerializer.Deserialize<string>(trimmed); }
            catch (JsonException) { return trimmed.Trim('"'); }
        }

        if (!trimmed.StartsWith('{'))
            return trimmed;

        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var messages = errors.EnumerateObject()
                    .SelectMany(p => p.Value.ValueKind == JsonValueKind.Array ? p.Value.EnumerateArray() : [])
                    .Where(e => e.ValueKind == JsonValueKind.String)
                    .Select(e => e.GetString()!)
                    .Where(s => s.Length > 0)
                    .Distinct()
                    .ToList();

                if (messages.Count > 0)
                    return string.Join(" ", messages);
            }

            foreach (var name in new[] { "detail", "title" })
            {
                if (root.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(prop.GetString()))
                    return prop.GetString();
            }
        }
        catch (JsonException)
        {
            return trimmed;
        }

        return null;
    }

    public static async Task<string?> ReadMessageAsync(HttpResponseMessage response, CancellationToken ct)
        => ExtractMessage(await response.Content.ReadAsStringAsync(ct));

    public static async Task<ApiException> ToExceptionAsync(HttpResponseMessage response, CancellationToken ct)
        => new(response.StatusCode, await ReadMessageAsync(response, ct));

    public static async Task<TokenExchangeException> ToExchangeExceptionAsync(HttpResponseMessage response, CancellationToken ct)
        => new(response.StatusCode, await ReadMessageAsync(response, ct));

    public static bool IsServerError(HttpStatusCode status) => (int)status >= 500;
}
