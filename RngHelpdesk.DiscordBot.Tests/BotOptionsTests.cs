using Microsoft.Extensions.Configuration;

namespace RngHelpdesk.DiscordBot.Tests;

public class BotOptionsTests
{
    private static BotOptions Bind(Dictionary<string, string?> values)
        => BotOptions.Bind(new ConfigurationBuilder().AddInMemoryCollection(values).Build());

    private static Dictionary<string, string?> Valid() => new()
    {
        ["Discord:BotToken"] = "t",
        ["Discord:GuildId"] = "123",
        ["Api:BaseUrl"] = "https://localhost:5081",
        ["Api:BotApiKey"] = "k",
    };

    [Fact]
    public void Valid_config_has_no_problems()
    {
        var options = Bind(Valid());

        Assert.Empty(options.Validate());
        Assert.Equal(123UL, options.GuildIdValue);
    }

    [Fact]
    public void Empty_config_reports_every_missing_setting()
    {
        var problems = Bind([]).Validate();

        Assert.Equal(4, problems.Count);
    }

    [Fact]
    public void Bad_guild_id_and_url_are_reported()
    {
        var values = Valid();
        values["Discord:GuildId"] = "not-a-number";
        values["Api:BaseUrl"] = "localhost";

        var problems = Bind(values).Validate();

        Assert.Contains(problems, p => p.Contains("GuildId"));
        Assert.Contains(problems, p => p.Contains("BaseUrl"));
    }

    [Fact]
    public void ThrowIfInvalid_names_the_missing_keys()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Bind([]).ThrowIfInvalid());

        Assert.Contains("Discord:BotToken", ex.Message);
        Assert.Contains("Api:BotApiKey", ex.Message);
    }
}
