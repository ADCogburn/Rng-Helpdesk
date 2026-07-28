using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users.Events;

namespace RngHelpdesk.Domain.Tests.Users;

public class UserDeactivateTests
{
    [Fact]
    public void Deactivate_ActingUserIdIsZero_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();

        Assert.Throws<DomainException>(() => user.Deactivate(actingUserId: 0));
    }

    [Fact]
    public void Deactivate_UserAlreadyInactive_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();
        user.Deactivate(TestUsers.DefaultActingUserId);

        Assert.Throws<DomainException>(() => user.Deactivate(TestUsers.DefaultActingUserId));
    }

    [Fact]
    public void Deactivate_ValidInput_RaisesUserDeactivatedEventAndSetsInactive()
    {
        var user = TestUsers.CreateValidUser();

        user.Deactivate(TestUsers.DefaultActingUserId);

        Assert.False(user.IsActive);

        var raisedEvent = Assert.Single(user.UncommittedDomainEvents);
        var deactivatedEvent = Assert.IsType<UserDeactivatedEvent>(raisedEvent);
        Assert.Equal(TestUsers.DefaultActingUserId, deactivatedEvent.ActingUserId);
        Assert.Equal(user.Id, deactivatedEvent.UserId);
    }
}
