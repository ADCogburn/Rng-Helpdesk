namespace RngHelpdesk.DiscordBot;

public sealed class DiscordOptions
{
    public string BotToken { get; set; } = "";
    public string GuildId { get; set; } = "";
}

public sealed class ApiOptions
{
    public string BaseUrl { get; set; } = "";
    public string BotApiKey { get; set; } = "";
}

public sealed class BotOptions
{
    public DiscordOptions Discord { get; set; } = new();
    public ApiOptions Api { get; set; } = new();

    public ulong GuildIdValue => ulong.Parse(Discord.GuildId);

    public static BotOptions Bind(IConfiguration configuration)
    {
        var options = new BotOptions();
        configuration.GetSection("Discord").Bind(options.Discord);
        configuration.GetSection("Api").Bind(options.Api);
        return options;
    }

    /// <summary>Returns a human-readable problem per missing/invalid setting; empty when valid.</summary>
    public IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(Discord.BotToken))
            problems.Add("Discord:BotToken is required (set it with `dotnet user-secrets set Discord:BotToken <token> --project RngHelpdesk.DiscordBot`).");

        if (string.IsNullOrWhiteSpace(Discord.GuildId))
            problems.Add("Discord:GuildId is required (the numeric id of the guild to register slash commands in).");
        else if (!ulong.TryParse(Discord.GuildId, out _))
            problems.Add("Discord:GuildId must be a numeric Discord snowflake.");

        if (string.IsNullOrWhiteSpace(Api.BaseUrl))
            problems.Add("Api:BaseUrl is required (e.g. https://localhost:5081).");
        else if (!Uri.TryCreate(Api.BaseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            problems.Add("Api:BaseUrl must be an absolute http(s) URL.");

        if (string.IsNullOrWhiteSpace(Api.BotApiKey))
            problems.Add("Api:BotApiKey is required and must match DiscordBot:ApiKey on the API.");

        return problems;
    }

    public void ThrowIfInvalid()
    {
        var problems = Validate();
        if (problems.Count > 0)
            throw new InvalidOperationException("RngHelpdesk.DiscordBot configuration is invalid:" + Environment.NewLine + " - " + string.Join(Environment.NewLine + " - ", problems));
    }
}
