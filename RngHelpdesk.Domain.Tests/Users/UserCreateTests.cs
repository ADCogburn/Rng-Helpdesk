using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Domain.Users.Events;

namespace RngHelpdesk.Domain.Tests.Users;

public class UserCreateTests
{
    [Fact]
    public void Create_ActingUserIdIsZero_ThrowsDomainException()
    {
        var discordAccount = TestUsers.ValidDiscordAccount();

        Assert.Throws<DomainException>(() =>
            User.Create(actingUserId: 0, discordAccount: discordAccount, runescapeAccounts: Array.Empty<RunescapeAccount>()));
    }

    [Fact]
    public void Create_DiscordAccountIsNull_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            User.Create(actingUserId: TestUsers.DefaultActingUserId, discordAccount: null!, runescapeAccounts: Array.Empty<RunescapeAccount>()));
    }

    // DiscordAccount's own constructor already rejects DiscordId == 0 and a null Username
    // (throwing ArgumentNullException before User.Create ever runs), so those two branches
    // of User.Create's guard are unreachable through the public API without reflection.
    // Only the whitespace-username branch is reachable and covered here.
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_DiscordUsernameIsWhitespace_ThrowsDomainException(string username)
    {
        var discordAccount = new DiscordAccount(TestUsers.DefaultDiscordId, username);

        Assert.Throws<DomainException>(() =>
            User.Create(actingUserId: TestUsers.DefaultActingUserId, discordAccount: discordAccount, runescapeAccounts: Array.Empty<RunescapeAccount>()));
    }

    [Fact]
    public void Create_ValidInput_RaisesUserCreatedEventAndSetsState()
    {
        var discordAccount = TestUsers.ValidDiscordAccount();
        var runescapeAccounts = new[] { new RunescapeAccount("Zezima") };
        var beforeCreate = DateTimeOffset.UtcNow;

        var user = User.Create(TestUsers.DefaultActingUserId, discordAccount, runescapeAccounts);

        Assert.Equal(discordAccount.DiscordId, user.Id);
        Assert.True(user.IsActive);
        Assert.Same(discordAccount, user.DiscordAccount);
        Assert.Equal(new[] { "Zezima" }, user.RunescapeAccounts.Select(a => a.Username));
        Assert.InRange(user.DateCreated, beforeCreate, DateTimeOffset.UtcNow);

        var raisedEvent = Assert.Single(user.UncommittedDomainEvents);
        var createdEvent = Assert.IsType<UserCreatedEvent>(raisedEvent);
        Assert.Equal(TestUsers.DefaultActingUserId, createdEvent.ActingUserId);
        Assert.Equal(discordAccount.DiscordId, createdEvent.UserId);
    }
}
