using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Domain.Tests.Users;

public class UserAddClanPointsTests
{
    private const string ValidReason = "Boss kill contribution";

    [Fact]
    public void AddClanPoints_ActingUserIdIsZero_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();

        Assert.Throws<DomainException>(() => user.AddClanPoints(actingUserId: 0, points: 10, reason: ValidReason));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddClanPoints_PointsIsNotPositive_ThrowsDomainException(int points)
    {
        var user = TestUsers.CreateValidUser();

        Assert.Throws<DomainException>(() => user.AddClanPoints(TestUsers.DefaultActingUserId, points, ValidReason));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddClanPoints_ReasonIsNullOrWhitespace_ThrowsDomainException(string? reason)
    {
        var user = TestUsers.CreateValidUser();

        Assert.Throws<DomainException>(() => user.AddClanPoints(TestUsers.DefaultActingUserId, 10, reason!));
    }

    [Fact]
    public void AddClanPoints_WouldOverflowMaxValue_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();
        user.AddClanPoints(TestUsers.DefaultActingUserId, int.MaxValue, ValidReason);

        Assert.Throws<DomainException>(() => user.AddClanPoints(TestUsers.DefaultActingUserId, 1, ValidReason));
    }

    [Fact]
    public void AddClanPoints_ValidInput_RaisesClanPointsChangedEventWithPositiveDelta()
    {
        var user = TestUsers.CreateValidUser();

        user.AddClanPoints(TestUsers.DefaultActingUserId, 50, ValidReason);

        var raisedEvent = Assert.Single(user.UncommittedDomainEvents);
        var pointsEvent = Assert.IsType<ClanPointsChangedEvent>(raisedEvent);
        Assert.Equal(TestUsers.DefaultActingUserId, pointsEvent.ActingUserId);
        Assert.Equal(user.Id, pointsEvent.UserId);
        Assert.Equal(50, pointsEvent.Delta);
        Assert.Equal(ValidReason, pointsEvent.Reason);
    }
}
