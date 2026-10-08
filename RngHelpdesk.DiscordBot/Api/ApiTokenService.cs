using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RngHelpdesk.DiscordBot.Api.Models;
using RngHelpdesk.DiscordBot.Api.Serialization;

namespace RngHelpdesk.DiscordBot.Api;

/// <summary>
/// Trades the shared API key for a bot JWT, then the bot JWT + a Discord id for a short-lived user JWT.
/// Both are cached in memory and refreshed <see cref="RefreshSkew"/> before they expire.
/// </summary>
public sealed class ApiTokenService(HttpClient http, string botApiKey, TimeProvider? timeProvider = null)
{
    public static readonly TimeSpan RefreshSkew = TimeSpan.FromMinutes(1);

    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly ConcurrentDictionary<ulong, CachedToken> _userTokens = new();
    private CachedToken? _botToken;

    private sealed record CachedToken(string Token, DateTimeOffset ExpiresAt);

    private bool IsFresh(CachedToken? token) => token is not null && token.ExpiresAt - RefreshSkew > _time.GetUtcNow();

    public async Task<string> GetUserTokenAsync(ulong discordId, CancellationToken ct = default)
    {
        if (_userTokens.TryGetValue(discordId, out var cached) && IsFresh(cached))
            return cached.Token;

        await _gate.WaitAsync(ct);
        try
        {
            if (_userTokens.TryGetValue(discordId, out cached) && IsFresh(cached))
                return cached.Token;

            var exchanged = await ExchangeAsync(discordId, ct);
            _userTokens[discordId] = new CachedToken(exchanged.Token, exchanged.ExpiresAt);
            return exchanged.Token;
        }
        finally
        {
            _gate.Release();
        }
    }

    public void InvalidateUserToken(ulong discordId) => _userTokens.TryRemove(discordId, out _);

    private async Task<DiscordExchangeResponse> ExchangeAsync(ulong discordId, CancellationToken ct)
    {
        // The bot JWT can be rejected if the API restarted with a different key; refresh it once.
        for (var attempt = 0; ; attempt++)
        {
            var botToken = await GetBotTokenAsync(ct);

            using var request = new HttpRequestMessage(HttpMethod.Post, "auth/discord")
            {
                Content = JsonContent.Create(new DiscordExchangeRequest(discordId), options: ApiJson.Options),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", botToken);

            using var response = await http.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DiscordExchangeResponse>(ApiJson.Options, ct)
                    ?? throw new ApiException(response.StatusCode, "Empty token exchange response.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && attempt == 0)
            {
                _botToken = null;
                continue;
            }

            throw await ApiErrorReader.ToExchangeExceptionAsync(response, ct);
        }
    }

    private async Task<string> GetBotTokenAsync(CancellationToken ct)
    {
        if (IsFresh(_botToken))
            return _botToken!.Token;

        using var response = await http.PostAsJsonAsync("auth/bot/token", new BotTokenRequest(botApiKey), ApiJson.Options, ct);

        if (!response.IsSuccessStatusCode)
            throw await ApiErrorReader.ToExceptionAsync(response, ct);

        var body = await response.Content.ReadFromJsonAsync<BotTokenResponse>(ApiJson.Options, ct)
            ?? throw new ApiException(response.StatusCode, "Empty bot token response.");

        _botToken = new CachedToken(body.Token, body.ExpiresAt);
        return body.Token;
    }
}
