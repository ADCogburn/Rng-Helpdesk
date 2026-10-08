using System.Net;
using RngHelpdesk.DiscordBot.Api;
using RngHelpdesk.DiscordBot.Api.Models;

namespace RngHelpdesk.DiscordBot.Tests;

public class RngApiClientTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);
    private const ulong Admin = 111111111111111111UL;
    private const ulong Target = 222222222222222222UL;

    private const string UserJson = """
        {"id":"222222222222222222","appRole":"Member","clanPoints":1500,"rank":"Steel","isActive":true,
         "dateCreated":"2026-01-02T03:04:05Z",
         "discordAccount":{"discordId":"222222222222222222","username":"bob"},
         "runescapeAccounts":[{"username":"Bob Iron"}]}
        """;

    /// <summary>Wires a real token service to the same scripted handler; auth endpoints always succeed.</summary>
    private static (RngApiClient Client, FakeHandler Handler) Create(Func<RecordedRequest, HttpResponseMessage> api)
    {
        var issued = 0;
        var handler = new FakeHandler(r => r.PathAndQuery switch
        {
            "/auth/bot/token" => FakeHandler.Json($$"""{"token":"bot","expiresAt":"{{Start.AddHours(1):O}}"}"""),
            "/auth/discord" => FakeHandler.Json($$"""{"token":"user-{{++issued}}","expiresAt":"{{Start.AddMinutes(15):O}}","appRole":"Administrator"}"""),
            _ => api(r),
        });
        var http = handler.CreateClient();
        var tokens = new ApiTokenService(http, "secret", new FakeTimeProvider(Start));
        return (new RngApiClient(http, tokens), handler);
    }

    private static RecordedRequest ApiCall(FakeHandler h) => h.Requests.Last(r => !r.PathAndQuery.StartsWith("/auth/bot") && r.PathAndQuery != "/auth/discord");

    [Fact]
    public async Task GetUser_sends_bearer_token_and_ids_round_trip_as_strings()
    {
        var (client, handler) = Create(_ => FakeHandler.Json(UserJson));

        var user = await client.GetUserAsync(Admin, Target);

        Assert.Equal("/users/222222222222222222", ApiCall(handler).PathAndQuery);
        Assert.Equal("Bearer user-1", ApiCall(handler).Authorization);
        Assert.Equal(Target, user.Id);
        Assert.Equal(Rank.Steel, user.Rank);
        Assert.Equal(AppRole.Member, user.AppRole);
        Assert.Equal(Target, user.DiscordAccount.DiscordId);
        Assert.Equal("Bob Iron", Assert.Single(user.RunescapeAccounts).Username);
    }

    [Fact]
    public async Task Public_endpoints_are_called_without_authorization_or_token_exchange()
    {
        var (client, handler) = Create(_ => FakeHandler.Json("""{"entries":[{"position":1,"runescapeUsername":"A","rank":"Zenyte","clanPoints":9}]}"""));

        var board = await client.GetLeaderboardAsync(500);

        Assert.Single(handler.Requests);
        Assert.Equal("/public/leaderboard?top=25", handler.Requests[0].PathAndQuery); // clamped 1..25
        Assert.Null(handler.Requests[0].Authorization);
        Assert.Equal(Rank.Zenyte, board.Entries[0].Rank);
    }

    [Fact]
    public async Task CreateMember_posts_string_discord_id_and_reads_credentials()
    {
        var (client, handler) = Create(_ => FakeHandler.Json("""{"userId":"222222222222222222","username":"bob","temporaryPassword":"pw123"}""", HttpStatusCode.Created));

        var created = await client.CreateMemberAsync(Admin, Target, "bob", ["Bob Iron"]);

        var call = ApiCall(handler);
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.Equal("/admin/create", call.PathAndQuery);
        Assert.Contains("\"discordId\":\"222222222222222222\"", call.Body);
        Assert.Contains("\"runescapeAccounts\":[{\"username\":\"Bob Iron\"}]", call.Body);
        Assert.Equal("pw123", created.TemporaryPassword);
        Assert.Equal(Target, created.UserId);
    }

    [Fact]
    public async Task Delink_sends_DELETE_with_json_body()
    {
        var (client, handler) = Create(_ => FakeHandler.Empty(HttpStatusCode.OK));

        await client.DelinkRunescapeAccountAsync(Admin, Target, "Bob Iron");

        var call = ApiCall(handler);
        Assert.Equal(HttpMethod.Delete, call.Method);
        Assert.Equal("/users/222222222222222222/runescape-accounts", call.PathAndQuery);
        Assert.Equal("""{"username":"Bob Iron"}""", call.Body);
    }

    [Fact]
    public async Task Rsn_lookup_escapes_the_path_segment()
    {
        var (client, handler) = Create(_ => FakeHandler.Json(UserJson));

        await client.GetUserByRsnAsync(Admin, "Bob Iron");

        Assert.Equal("/users/by-rsn/Bob%20Iron", ApiCall(handler).PathAndQuery);
    }

    [Fact]
    public async Task Points_and_threshold_requests_use_expected_routes_and_bodies()
    {
        var (client, handler) = Create(_ => FakeHandler.Empty(HttpStatusCode.NoContent));

        await client.AddPointsAsync(Admin, Target, 50, "event");
        Assert.Equal("/users/222222222222222222/points/add", ApiCall(handler).PathAndQuery);
        Assert.Equal("""{"points":50,"reason":"event"}""", ApiCall(handler).Body);

        await client.SetRankThresholdAsync(Admin, Rank.Rune, 4000);
        Assert.Equal(HttpMethod.Put, ApiCall(handler).Method);
        Assert.Equal("/rankthresholds/Rune", ApiCall(handler).PathAndQuery);
        Assert.Equal("""{"pointsRequired":4000}""", ApiCall(handler).Body);

        await client.PromoteAsync(Admin, Target);
        Assert.Equal("/admin/222222222222222222/promote", ApiCall(handler).PathAndQuery);
    }

    [Fact]
    public async Task Unauthorized_drops_cached_token_and_retries_once_with_a_fresh_one()
    {
        var calls = 0;
        var (client, handler) = Create(r => ++calls == 1
            ? FakeHandler.Empty(HttpStatusCode.Unauthorized)
            : FakeHandler.Json(UserJson));

        var user = await client.GetMeAsync(Admin);

        Assert.Equal(Target, user.Id);
        var apiCalls = handler.Requests.Where(r => r.PathAndQuery == "/auth/me").ToList();
        Assert.Equal(["Bearer user-1", "Bearer user-2"], apiCalls.Select(r => r.Authorization));
    }

    [Fact]
    public async Task Persistent_unauthorized_throws_after_a_single_retry()
    {
        var (client, handler) = Create(_ => FakeHandler.Empty(HttpStatusCode.Unauthorized));

        var ex = await Assert.ThrowsAsync<ApiException>(() => client.GetMeAsync(Admin));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal(2, handler.Requests.Count(r => r.PathAndQuery == "/auth/me"));
    }

    [Fact]
    public async Task Forbidden_throws_ApiException_without_retry()
    {
        var (client, handler) = Create(_ => FakeHandler.Empty(HttpStatusCode.Forbidden));

        var ex = await Assert.ThrowsAsync<ApiException>(() => client.GetRankThresholdsAsync(Admin));

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.Equal(1, handler.Requests.Count(r => r.PathAndQuery == "/rankthresholds"));
    }

    [Fact]
    public async Task NotFound_plain_text_body_becomes_the_exception_message()
    {
        var (client, _) = Create(_ => FakeHandler.Text("User 5 not found.", HttpStatusCode.NotFound));

        var ex = await Assert.ThrowsAsync<ApiException>(() => client.GetUserAsync(Admin, 5));

        Assert.Equal("User 5 not found.", ex.Message);
        Assert.True(ex.HasApiMessage);
    }

    [Fact]
    public async Task BadRequest_validation_problem_details_are_flattened()
    {
        const string problem = """{"title":"One or more validation errors occurred.","status":400,"errors":{"Username":["Invalid RSN."],"Other":["Too long."]}}""";
        var (client, _) = Create(_ => FakeHandler.Json(problem, HttpStatusCode.BadRequest));

        var ex = await Assert.ThrowsAsync<ApiException>(() => client.LinkRunescapeAccountAsync(Admin, Target, "!!"));

        Assert.Equal("Invalid RSN. Too long.", ex.Message);
    }

    [Fact]
    public async Task Server_error_surfaces_as_ApiException_500()
    {
        var (client, _) = Create(_ => FakeHandler.Empty(HttpStatusCode.InternalServerError));

        var ex = await Assert.ThrowsAsync<ApiException>(() => client.GetOverviewAsync());

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public async Task Unregistered_user_surfaces_TokenExchangeException()
    {
        var handler = new FakeHandler(r => r.PathAndQuery == "/auth/bot/token"
            ? FakeHandler.Json($$"""{"token":"bot","expiresAt":"{{Start.AddHours(1):O}}"}""")
            : FakeHandler.Empty(HttpStatusCode.NotFound));
        var http = handler.CreateClient();
        var client = new RngApiClient(http, new ApiTokenService(http, "secret", new FakeTimeProvider(Start)));

        var ex = await Assert.ThrowsAsync<TokenExchangeException>(() => client.GetMeAsync(Admin));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }
}
