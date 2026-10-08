using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users.Events;

namespace RngHelpdesk.Domain.Tests.Users;

public class UserRenameRunescapeAccountTests
{
    [Fact]
    public void RenameRunescapeAccount_ActingUserIdIsZero_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");

        Assert.Throws<DomainException>(() =>
            user.RenameRunescapeAccount(actingUserId: 0, oldUsername: "Zezima", newUsername: "Zezima2"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void RenameRunescapeAccount_OldUsernameIsNullOrWhitespace_ThrowsDomainException(string? oldUsername)
    {
        var user = TestUsers.CreateValidUser();
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");

        Assert.Throws<DomainException>(() =>
            user.RenameRunescapeAccount(TestUsers.DefaultActingUserId, oldUsername!, "Zezima2"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void RenameRunescapeAccount_NewUsernameIsNullOrWhitespace_ThrowsDomainException(string? newUsername)
    {
        var user = TestUsers.CreateValidUser();
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");

        Assert.Throws<DomainException>(() =>
            user.RenameRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima", newUsername!));
    }

    [Fact]
    public void RenameRunescapeAccount_OldUsernameNotFound_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();

        Assert.Throws<DomainException>(() =>
            user.RenameRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima", "Zezima2"));
    }

    [Fact]
    public void RenameRunescapeAccount_NewUsernameAlreadyExistsCaseInsensitive_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Woox");

        Assert.Throws<DomainException>(() =>
            user.RenameRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima", "WOOX"));
    }

    [Fact]
    public void RenameRunescapeAccount_ValidInput_RaisesRunescapeAccountRenamedEventWithOriginalCasing()
    {
        var user = TestUsers.CreateValidUser();
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");
        user.ClearUncommittedDomainEvents();

        user.RenameRunescapeAccount(TestUsers.DefaultActingUserId, "ZEZIMA", "Zezima2");

        Assert.Equal(new[] { "Zezima2" }, user.RunescapeAccounts.Select(a => a.Username));

        var raisedEvent = Assert.Single(user.UncommittedDomainEvents);
        var renamedEvent = Assert.IsType<RunescapeAccountRenamedEvent>(raisedEvent);
        Assert.Equal(TestUsers.DefaultActingUserId, renamedEvent.ActingUserId);
        Assert.Equal(user.Id, renamedEvent.UserId);
        Assert.Equal("Zezima", renamedEvent.OldUsername);
        Assert.Equal("Zezima2", renamedEvent.NewUsername);
    }
}
