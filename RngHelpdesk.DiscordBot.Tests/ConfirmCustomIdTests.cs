using RngHelpdesk.DiscordBot.Interactions.Confirm;

namespace RngHelpdesk.DiscordBot.Tests;

public class ConfirmCustomIdTests
{
    [Theory]
    [InlineData(ConfirmAction.Deactivate)]
    [InlineData(ConfirmAction.Promote)]
    [InlineData(ConfirmAction.Demote)]
    public void Confirm_id_round_trips(ConfirmAction action)
    {
        var id = ConfirmCustomId.Confirm(action, 222222222222222222UL, 111111111111111111UL);
        var parts = id.Split(':');

        Assert.Equal("confirm", parts[0]);
        Assert.True(ConfirmCustomId.TryParse(parts[1], parts[2], parts[3], out var a, out var target, out var invoker));
        Assert.Equal(action, a);
        Assert.Equal(222222222222222222UL, target);
        Assert.Equal(111111111111111111UL, invoker);
    }

    [Theory]
    [InlineData("explode", "1", "2")]
    [InlineData("promote", "abc", "2")]
    [InlineData("promote", "1", "")]
    [InlineData("42", "1", "2")]
    public void Garbage_is_rejected(string action, string target, string invoker)
        => Assert.False(ConfirmCustomId.TryParse(action, target, invoker, out _, out _, out _));

    [Fact]
    public void Cancel_id_carries_invoker()
        => Assert.Equal("cancel:99", ConfirmCustomId.Cancel(99));

    [Fact]
    public void Custom_ids_fit_discord_limit()
        => Assert.True(ConfirmCustomId.Confirm(ConfirmAction.Deactivate, ulong.MaxValue, ulong.MaxValue).Length <= 100);
}
