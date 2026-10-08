using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users.Events;

namespace RngHelpdesk.Domain.Tests.Users;

public class UserRemoveRunescapeAccountTests
{
    [Fact]
    public void RemoveRunescapeAccount_ActingUserIdIsZero_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");

        Assert.Throws<DomainException>(() => user.RemoveRunescapeAccount(actingUserId: 0, username: "Zezima"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void RemoveRunescapeAccount_UsernameIsNullOrWhitespace_ThrowsDomainException(string? username)
    {
        var user = TestUsers.CreateValidUser();
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");

        Assert.Throws<DomainException>(() => user.RemoveRunescapeAccount(TestUsers.DefaultActingUserId, username!));
    }

    [Fact]
    public void RemoveRunescapeAccount_UsernameNotLinked_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();

        Assert.Throws<DomainException>(() => user.RemoveRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima"));
    }

    [Fact]
    public void RemoveRunescapeAccount_ValidInput_RaisesRunescapeAccountDelinkedEventAndRemovesAccount()
    {
        var user = TestUsers.CreateValidUser();
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");
        user.ClearUncommittedDomainEvents();

        user.RemoveRunescapeAccount(TestUsers.DefaultActingUserId, "ZEZIMA");

        Assert.Empty(user.RunescapeAccounts);

        var raisedEvent = Assert.Single(user.UncommittedDomainEvents);
        var delinkedEvent = Assert.IsType<RunescapeAccountDelinkedEvent>(raisedEvent);
        Assert.Equal(TestUsers.DefaultActingUserId, delinkedEvent.ActingUserId);
        Assert.Equal(user.Id, delinkedEvent.UserId);
        Assert.Equal("Zezima", delinkedEvent.Username);
    }
}
