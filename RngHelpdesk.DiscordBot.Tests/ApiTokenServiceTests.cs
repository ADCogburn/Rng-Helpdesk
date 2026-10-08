using System.Net;
using RngHelpdesk.DiscordBot.Api;

namespace RngHelpdesk.DiscordBot.Tests;

public class ApiTokenServiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);

    private static string Expiry(DateTimeOffset at) => at.ToString("O");

    /// <summary>Handler issuing bot token "bot-N" and user token "user-N" on each call.</summary>
    private static FakeHandler Happy(FakeTimeProvider time, TimeSpan botLife, TimeSpan userLife)
    {
        var bot = 0;
        var user = 0;
        return new FakeHandler(r => r.PathAndQuery switch
        {
            "/auth/bot/token" => FakeHandler.Json($$"""{"token":"bot-{{++bot}}","expiresAt":"{{Expiry(time.GetUtcNow() + botLife)}}"}"""),
            "/auth/discord" => FakeHandler.Json($$"""{"token":"user-{{++user}}","expiresAt":"{{Expiry(time.GetUtcNow() + userLife)}}","appRole":"Administrator"}"""),
            _ => FakeHandler.Empty(HttpStatusCode.NotFound),
        });
    }

    [Fact]
    public async Task Exchange_sends_api_key_then_bot_token_with_discord_id_as_string()
    {
        var time = new FakeTimeProvider(Start);
        var handler = Happy(time, TimeSpan.FromHours(1), TimeSpan.FromMinutes(15));
        var service = new ApiTokenService(handler.CreateClient(), "secret", time);

        var token = await service.GetUserTokenAsync(123456789012345678UL);

        Assert.Equal("user-1", token);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Contains("\"apiKey\":\"secret\"", handler.Requests[0].Body);
        Assert.Null(handler.Requests[0].Authorization);
        Assert.Equal("Bearer bot-1", handler.Requests[1].Authorization);
        Assert.Contains("\"discordId\":\"123456789012345678\"", handler.Requests[1].Body);
    }

    [Fact]
    public async Task Tokens_are_cached_until_one_minute_before_expiry()
    {
        var time = new FakeTimeProvider(Start);
        var handler = Happy(time, TimeSpan.FromHours(1), TimeSpan.FromMinutes(15));
        var service = new ApiTokenService(handler.CreateClient(), "secret", time);

        Assert.Equal("user-1", await service.GetUserTokenAsync(1));
        time.Advance(TimeSpan.FromMinutes(13));
        Assert.Equal("user-1", await service.GetUserTokenAsync(1));
        Assert.Equal(2, handler.Requests.Count);

        time.Advance(TimeSpan.FromMinutes(1.5)); // inside the 1-minute refresh skew
        Assert.Equal("user-2", await service.GetUserTokenAsync(1));
        Assert.Equal(3, handler.Requests.Count); // bot token still fresh, only the exchange repeated
    }

    [Fact]
    public async Task Bot_token_is_shared_across_users()
    {
        var time = new FakeTimeProvider(Start);
        var handler = Happy(time, TimeSpan.FromHours(1), TimeSpan.FromMinutes(15));
        var service = new ApiTokenService(handler.CreateClient(), "secret", time);

        await service.GetUserTokenAsync(1);
        await service.GetUserTokenAsync(2);

        Assert.Equal(1, handler.Requests.Count(r => r.PathAndQuery == "/auth/bot/token"));
        Assert.Equal(2, handler.Requests.Count(r => r.PathAndQuery == "/auth/discord"));
    }

    [Fact]
    public async Task Invalidate_forces_a_new_exchange()
    {
        var time = new FakeTimeProvider(Start);
        var handler = Happy(time, TimeSpan.FromHours(1), TimeSpan.FromMinutes(15));
        var service = new ApiTokenService(handler.CreateClient(), "secret", time);

        await service.GetUserTokenAsync(1);
        service.InvalidateUserToken(1);

        Assert.Equal("user-2", await service.GetUserTokenAsync(1));
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task Exchange_failure_throws_TokenExchangeException_with_status(HttpStatusCode status)
    {
        var handler = new FakeHandler(r => r.PathAndQuery == "/auth/bot/token"
            ? FakeHandler.Json($$"""{"token":"b","expiresAt":"{{Expiry(Start.AddHours(1))}}"}""")
            : FakeHandler.Empty(status));
        var service = new ApiTokenService(handler.CreateClient(), "secret", new FakeTimeProvider(Start));

        var ex = await Assert.ThrowsAsync<TokenExchangeException>(() => service.GetUserTokenAsync(1));

        Assert.Equal(status, ex.StatusCode);
    }

    [Fact]
    public async Task Rejected_bot_token_is_refreshed_once_during_exchange()
    {
        var time = new FakeTimeProvider(Start);
        var botIssued = 0;
        var handler = new FakeHandler(r =>
        {
            if (r.PathAndQuery == "/auth/bot/token")
                return FakeHandler.Json($$"""{"token":"bot-{{++botIssued}}","expiresAt":"{{Expiry(Start.AddHours(1))}}"}""");
            return r.Authorization == "Bearer bot-2"
                ? FakeHandler.Json($$"""{"token":"user-ok","expiresAt":"{{Expiry(Start.AddMinutes(15))}}","appRole":"Member"}""")
                : FakeHandler.Empty(HttpStatusCode.Unauthorized);
        });
        var service = new ApiTokenService(handler.CreateClient(), "secret", time);

        Assert.Equal("user-ok", await service.GetUserTokenAsync(1));
        Assert.Equal(2, botIssued);
    }

    [Fact]
    public async Task Wrong_api_key_throws_ApiException_401()
    {
        var handler = new FakeHandler(_ => FakeHandler.Empty(HttpStatusCode.Unauthorized));
        var service = new ApiTokenService(handler.CreateClient(), "wrong", new FakeTimeProvider(Start));

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.GetUserTokenAsync(1));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.IsNotType<TokenExchangeException>(ex);
    }
}
