using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using RngHelpdesk.DiscordBot;
using RngHelpdesk.DiscordBot.Api;

var builder = WebApplication.CreateBuilder(args);

var botOptions = BotOptions.Bind(builder.Configuration);
botOptions.ThrowIfInvalid(); // fail fast with a clear message on missing config

builder.Services.AddSingleton(Options.Create(botOptions));

// Gateway client: Guilds intent only (no privileged intents needed for slash commands).
builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds,
}));
builder.Services.AddSingleton(sp => new InteractionService(sp.GetRequiredService<DiscordSocketClient>().Rest));

// API client: one shared HttpClient for token exchange and API calls.
var apiClientBuilder = builder.Services.AddHttpClient("rng-api", client =>
{
    client.BaseAddress = new Uri(botOptions.Api.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

if (builder.Environment.IsDevelopment())
{
    // Dev only: trust the ASP.NET dev certificate for localhost, mirroring the Vite proxy's `secure: false`.
    apiClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (request, _, _, errors) =>
            errors == System.Net.Security.SslPolicyErrors.None || request?.RequestUri?.IsLoopback == true,
    });
}

builder.Services.AddSingleton(sp => new ApiTokenService(
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("rng-api"),
    botOptions.Api.BotApiKey));
builder.Services.AddSingleton(sp => new RngApiClient(
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("rng-api"),
    sp.GetRequiredService<ApiTokenService>()));

builder.Services.AddHostedService<DiscordBotService>();

var app = builder.Build();

// Username resolver for the main API (reserved wiring, see API Program.cs). Backed by the gateway
// client's REST client rather than a second login.
app.MapGet("/discord/users/{discordId:long}", async (DiscordSocketClient client, long discordId) =>
{
    try
    {
        var user = await client.Rest.GetUserAsync((ulong)discordId, RequestOptions.Default);
        if (user is null)
            return Results.NotFound();

        // Prefer GlobalName (display name), fall back to Username
        var displayName = !string.IsNullOrEmpty(user.GlobalName)
            ? user.GlobalName
            : user.Username;

        return Results.Ok(new { Username = displayName });
    }
    catch (Exception)
    {
        return Results.NotFound();
    }
});

await app.RunAsync();
