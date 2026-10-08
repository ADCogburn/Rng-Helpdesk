using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users.Events;

namespace RngHelpdesk.Domain.Tests.Users;

public class UserAddRunescapeAccountTests
{
    [Fact]
    public void AddRunescapeAccount_ActingUserIdIsZero_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();

        Assert.Throws<DomainException>(() => user.AddRunescapeAccount(actingUserId: 0, username: "Zezima"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddRunescapeAccount_UsernameIsNullOrWhitespace_ThrowsDomainException(string? username)
    {
        var user = TestUsers.CreateValidUser();

        Assert.Throws<DomainException>(() => user.AddRunescapeAccount(TestUsers.DefaultActingUserId, username!));
    }

    [Fact]
    public void AddRunescapeAccount_UsernameAlreadyLinkedCaseInsensitive_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");

        Assert.Throws<DomainException>(() => user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "ZEZIMA"));
    }

    [Fact]
    public void AddRunescapeAccount_ValidInput_RaisesRunescapeAccountLinkedEventAndAddsAccount()
    {
        var user = TestUsers.CreateValidUser();

        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");

        Assert.Equal(new[] { "Zezima" }, user.RunescapeAccounts.Select(a => a.Username));

        var raisedEvent = Assert.Single(user.UncommittedDomainEvents);
        var linkedEvent = Assert.IsType<RunescapeAccountLinkedEvent>(raisedEvent);
        Assert.Equal(TestUsers.DefaultActingUserId, linkedEvent.ActingUserId);
        Assert.Equal(user.Id, linkedEvent.UserId);
        Assert.Equal("Zezima", linkedEvent.Username);
    }
}
