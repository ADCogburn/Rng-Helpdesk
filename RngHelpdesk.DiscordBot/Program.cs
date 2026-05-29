using Discord;
using Discord.Rest;

var builder = WebApplication.CreateBuilder(args);

// Discord REST client for resolving user IDs to usernames
var botToken = builder.Configuration["Discord:BotToken"];
if (string.IsNullOrEmpty(botToken))
    throw new InvalidOperationException("Discord:BotToken is required in configuration.");

var discordRest = new DiscordRestClient();
await discordRest.LoginAsync(TokenType.Bot, botToken);
builder.Services.AddSingleton(discordRest);

var app = builder.Build();

app.MapGet("/discord/users/{discordId:long}", async (DiscordRestClient restClient, ulong discordId, CancellationToken ct) =>
{
    try
    {
        var user = await restClient.GetUserAsync(discordId, RequestOptions.Default);
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
