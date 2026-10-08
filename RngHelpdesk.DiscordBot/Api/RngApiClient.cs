using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RngHelpdesk.DiscordBot.Api.Models;
using RngHelpdesk.DiscordBot.Api.Serialization;

namespace RngHelpdesk.DiscordBot.Api;

/// <summary>
/// Typed client for the RngHelpdesk API. Every non-public method takes the Discord id of the invoking
/// user and calls the API as them (per-user token exchange), so the API's own authorization applies.
/// Failures surface as <see cref="ApiException"/> (or <see cref="TokenExchangeException"/>).
/// </summary>
public sealed class RngApiClient(HttpClient http, ApiTokenService tokens)
{
    // ---- public (anonymous) ----

    public Task<PublicOverviewResponse> GetOverviewAsync(CancellationToken ct = default)
        => SendAsync<PublicOverviewResponse>(null, () => new(HttpMethod.Get, "public/overview"), ct);

    public Task<PublicRanksResponse> GetRanksAsync(CancellationToken ct = default)
        => SendAsync<PublicRanksResponse>(null, () => new(HttpMethod.Get, "public/ranks"), ct);

    public Task<PublicLeaderboardResponse> GetLeaderboardAsync(int top, CancellationToken ct = default)
        => SendAsync<PublicLeaderboardResponse>(null, () => new(HttpMethod.Get, $"public/leaderboard?top={Math.Clamp(top, 1, 25)}"), ct);

    // ---- self ----

    public Task<UserResponse> GetMeAsync(ulong actor, CancellationToken ct = default)
        => SendAsync<UserResponse>(actor, () => new(HttpMethod.Get, "auth/me"), ct);

    public Task<PointHistoryResponse> GetMyPointHistoryAsync(ulong actor, CancellationToken ct = default)
        => SendAsync<PointHistoryResponse>(actor, () => new(HttpMethod.Get, "auth/me/point-history"), ct);

    // ---- users ----

    public Task<UserResponse> GetUserAsync(ulong actor, ulong userId, CancellationToken ct = default)
        => SendAsync<UserResponse>(actor, () => new(HttpMethod.Get, $"users/{userId}"), ct);

    public Task<UserResponse> GetUserByRsnAsync(ulong actor, string rsn, CancellationToken ct = default)
        => SendAsync<UserResponse>(actor, () => new(HttpMethod.Get, $"users/by-rsn/{Uri.EscapeDataString(rsn)}"), ct);

    public Task<UsersByHistoricalRsnResponse> GetUsersByHistoricalRsnAsync(ulong actor, string rsn, CancellationToken ct = default)
        => SendAsync<UsersByHistoricalRsnResponse>(actor, () => new(HttpMethod.Get, $"users/by-historical-rsn/{Uri.EscapeDataString(rsn)}"), ct);

    public Task<UserLifecycleResponse> GetLifecycleAsync(ulong actor, ulong userId, CancellationToken ct = default)
        => SendAsync<UserLifecycleResponse>(actor, () => new(HttpMethod.Get, $"users/{userId}/lifecycle"), ct);

    public Task<PointHistoryResponse> GetPointHistoryAsync(ulong actor, ulong userId, CancellationToken ct = default)
        => SendAsync<PointHistoryResponse>(actor, () => new(HttpMethod.Get, $"users/{userId}/point-history"), ct);

    public Task AddPointsAsync(ulong actor, ulong userId, int points, string reason, CancellationToken ct = default)
        => SendAsync(actor, () => Json(HttpMethod.Post, $"users/{userId}/points/add", new AdjustPointsRequest(points, reason)), ct);

    public Task RemovePointsAsync(ulong actor, ulong userId, int points, string reason, CancellationToken ct = default)
        => SendAsync(actor, () => Json(HttpMethod.Post, $"users/{userId}/points/remove", new AdjustPointsRequest(points, reason)), ct);

    // ---- runescape accounts ----

    public Task<RunescapeAccountsResponse> GetRunescapeAccountsAsync(ulong actor, ulong userId, CancellationToken ct = default)
        => SendAsync<RunescapeAccountsResponse>(actor, () => new(HttpMethod.Get, $"users/{userId}/runescape-accounts"), ct);

    public Task<RunescapeAccountsResponse> GetPreviousRunescapeAccountsAsync(ulong actor, ulong userId, CancellationToken ct = default)
        => SendAsync<RunescapeAccountsResponse>(actor, () => new(HttpMethod.Get, $"users/{userId}/runescape-accounts/previous"), ct);

    public Task<RunescapeAccountHistoryResponse> GetRunescapeAccountHistoryAsync(ulong actor, ulong userId, CancellationToken ct = default)
        => SendAsync<RunescapeAccountHistoryResponse>(actor, () => new(HttpMethod.Get, $"users/{userId}/runescape-accounts/history"), ct);

    public Task LinkRunescapeAccountAsync(ulong actor, ulong userId, string rsn, CancellationToken ct = default)
        => SendAsync(actor, () => Json(HttpMethod.Post, $"users/{userId}/runescape-accounts", new RunescapeUsernameRequest(rsn)), ct);

    public Task DelinkRunescapeAccountAsync(ulong actor, ulong userId, string rsn, CancellationToken ct = default)
        => SendAsync(actor, () => Json(HttpMethod.Delete, $"users/{userId}/runescape-accounts", new RunescapeUsernameRequest(rsn)), ct);

    public Task RenameRunescapeAccountAsync(ulong actor, ulong userId, string oldRsn, string newRsn, CancellationToken ct = default)
        => SendAsync(actor, () => Json(HttpMethod.Put, $"users/{userId}/runescape-accounts/rename", new RenameRunescapeAccountRequest(oldRsn, newRsn)), ct);

    // ---- admin ----

    public Task<CreateUserResponse> CreateMemberAsync(ulong actor, ulong discordId, string discordUsername, IEnumerable<string> rsns, CancellationToken ct = default)
    {
        var body = new CreateUserRequest(
            new DiscordAccountDto(discordId, discordUsername),
            rsns.Select(r => new RunescapeAccountDto(r)).ToList());
        return SendAsync<CreateUserResponse>(actor, () => Json(HttpMethod.Post, "admin/create", body), ct);
    }

    public Task PromoteAsync(ulong actor, ulong userId, CancellationToken ct = default)
        => SendAsync(actor, () => new(HttpMethod.Post, $"admin/{userId}/promote"), ct);

    public Task DemoteAsync(ulong actor, ulong userId, CancellationToken ct = default)
        => SendAsync(actor, () => new(HttpMethod.Post, $"admin/{userId}/demote"), ct);

    public Task DeactivateAsync(ulong actor, ulong userId, CancellationToken ct = default)
        => SendAsync(actor, () => new(HttpMethod.Post, $"admin/{userId}/deactivate"), ct);

    public Task ReactivateAsync(ulong actor, ulong userId, CancellationToken ct = default)
        => SendAsync(actor, () => new(HttpMethod.Post, $"admin/{userId}/reactivate"), ct);

    // ---- rank thresholds ----

    public Task<RankThresholdsResponse> GetRankThresholdsAsync(ulong actor, CancellationToken ct = default)
        => SendAsync<RankThresholdsResponse>(actor, () => new(HttpMethod.Get, "rankthresholds"), ct);

    public Task SetRankThresholdAsync(ulong actor, Rank rank, int pointsRequired, CancellationToken ct = default)
        => SendAsync(actor, () => Json(HttpMethod.Put, $"rankthresholds/{rank}", new UpdateRankThresholdRequest(pointsRequired)), ct);

    // ---- plumbing ----

    private static HttpRequestMessage Json<T>(HttpMethod method, string url, T body)
        => new(method, url) { Content = JsonContent.Create(body, options: ApiJson.Options) };

    private async Task<T> SendAsync<T>(ulong? actor, Func<HttpRequestMessage> build, CancellationToken ct)
    {
        using var response = await SendCoreAsync(actor, build, ct);
        return await response.Content.ReadFromJsonAsync<T>(ApiJson.Options, ct)
            ?? throw new ApiException(response.StatusCode, "The API returned an empty response.");
    }

    private async Task SendAsync(ulong? actor, Func<HttpRequestMessage> build, CancellationToken ct)
    {
        using var response = await SendCoreAsync(actor, build, ct);
    }

    /// <summary>Sends as <paramref name="actor"/>; on a 401 drops the cached user token and retries once.</summary>
    private async Task<HttpResponseMessage> SendCoreAsync(ulong? actor, Func<HttpRequestMessage> build, CancellationToken ct)
    {
        for (var attempt = 0; ; attempt++)
        {
            using var request = build();

            if (actor is { } id)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await tokens.GetUserTokenAsync(id, ct));

            var response = await http.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
                return response;

            if (response.StatusCode == HttpStatusCode.Unauthorized && actor is { } actorId && attempt == 0)
            {
                response.Dispose();
                tokens.InvalidateUserToken(actorId);
                continue;
            }

            using (response)
                throw await ApiErrorReader.ToExceptionAsync(response, ct);
        }
    }
}
