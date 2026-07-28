using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users.Events;

namespace RngHelpdesk.Domain.Tests.Users;

public class UserReactivateTests
{
    [Fact]
    public void Reactivate_ActingUserIdIsZero_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();
        user.Deactivate(TestUsers.DefaultActingUserId);

        Assert.Throws<DomainException>(() => user.Reactivate(actingUserId: 0));
    }

    [Fact]
    public void Reactivate_UserAlreadyActive_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();

        Assert.Throws<DomainException>(() => user.Reactivate(TestUsers.DefaultActingUserId));
    }

    [Fact]
    public void Reactivate_ValidInput_RaisesUserReactivatedEventAndSetsActive()
    {
        var user = TestUsers.CreateValidUser();
        user.Deactivate(TestUsers.DefaultActingUserId);
        user.ClearUncommittedDomainEvents();

        user.Reactivate(TestUsers.DefaultActingUserId);

        Assert.True(user.IsActive);

        var raisedEvent = Assert.Single(user.UncommittedDomainEvents);
        var reactivatedEvent = Assert.IsType<UserReactivatedEvent>(raisedEvent);
        Assert.Equal(TestUsers.DefaultActingUserId, reactivatedEvent.ActingUserId);
        Assert.Equal(user.Id, reactivatedEvent.UserId);
    }
}
