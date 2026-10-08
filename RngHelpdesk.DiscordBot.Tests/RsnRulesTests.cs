using RngHelpdesk.DiscordBot.Interactions;

namespace RngHelpdesk.DiscordBot.Tests;

public class RsnRulesTests
{
    [Theory]
    [InlineData("Zezima")]
    [InlineData("a")]
    [InlineData("Bob Iron-1")]
    [InlineData("123456789012")]
    public void Valid_names(string rsn) => Assert.True(RsnRules.IsValid(rsn));

    [Theory]
    [InlineData("")]
    [InlineData(" lead")]
    [InlineData("trail ")]
    [InlineData("1234567890123")]
    [InlineData("under_score")]
    [InlineData("new\nline")]
    [InlineData("trail\n")]
    [InlineData(null)]
    public void Invalid_names(string? rsn) => Assert.False(RsnRules.IsValid(rsn));

    [Fact]
    public void Suggest_filters_by_case_insensitive_prefix_and_dedupes()
    {
        var result = RsnRules.Suggest(["Bob Iron", "bob iron", "Alice", "Bobby"], "bo");

        Assert.Equal(["Bob Iron", "Bobby"], result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Suggest_with_empty_input_returns_all(string? typed)
        => Assert.Equal(["A", "B"], RsnRules.Suggest(["A", "B"], typed));

    [Fact]
    public void Suggest_is_capped_at_25()
    {
        var names = Enumerable.Range(0, 40).Select(i => $"Name{i}");

        Assert.Equal(25, RsnRules.Suggest(names, "name").Count);
    }
}
