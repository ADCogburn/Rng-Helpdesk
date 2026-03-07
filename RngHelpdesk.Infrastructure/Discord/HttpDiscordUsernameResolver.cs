using System.Net.Http.Json;
using RngHelpdesk.Contracts.Discord;

namespace RngHelpdesk.Infrastructure.Discord;

public sealed class HttpDiscordUsernameResolver : IDiscordUsernameResolver
{
    private readonly HttpClient _httpClient;

    public HttpDiscordUsernameResolver(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> ResolveUsernameAsync(ulong discordId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/discord/users/{discordId}", ct);
            if (!response.IsSuccessStatusCode)
                return null;

            var dto = await response.Content.ReadFromJsonAsync<DiscordUserResponse>(ct);
            return dto?.Username;
        }
        catch
        {
            return null;
        }
    }

    private sealed record DiscordUserResponse(string Username);
}
