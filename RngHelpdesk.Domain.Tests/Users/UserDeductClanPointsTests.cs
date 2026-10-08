using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Domain.Tests.Users;

public class UserDeductClanPointsTests
{
    private const string ValidReason = "Penalty for rule violation";

    [Fact]
    public void DeductClanPoints_ActingUserIdIsZero_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();
        user.AddClanPoints(TestUsers.DefaultActingUserId, 10, ValidReason);

        Assert.Throws<DomainException>(() => user.DeductClanPoints(actingUserId: 0, points: 5, reason: ValidReason));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeductClanPoints_PointsIsNotPositive_ThrowsDomainException(int points)
    {
        var user = TestUsers.CreateValidUser();
        user.AddClanPoints(TestUsers.DefaultActingUserId, 10, ValidReason);

        Assert.Throws<DomainException>(() => user.DeductClanPoints(TestUsers.DefaultActingUserId, points, ValidReason));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void DeductClanPoints_ReasonIsNullOrWhitespace_ThrowsDomainException(string? reason)
    {
        var user = TestUsers.CreateValidUser();
        user.AddClanPoints(TestUsers.DefaultActingUserId, 10, ValidReason);

        Assert.Throws<DomainException>(() => user.DeductClanPoints(TestUsers.DefaultActingUserId, 5, reason!));
    }

    [Fact]
    public void DeductClanPoints_WouldGoBelowZero_ThrowsDomainException()
    {
        var user = TestUsers.CreateValidUser();
        user.AddClanPoints(TestUsers.DefaultActingUserId, 100, ValidReason);

        Assert.Throws<DomainException>(() => user.DeductClanPoints(TestUsers.DefaultActingUserId, 101, ValidReason));
    }

    [Fact]
    public void DeductClanPoints_ValidInput_RaisesClanPointsChangedEventWithNegativeDelta()
    {
        var user = TestUsers.CreateValidUser();
        user.AddClanPoints(TestUsers.DefaultActingUserId, 100, ValidReason);
        user.ClearUncommittedDomainEvents();

        user.DeductClanPoints(TestUsers.DefaultActingUserId, 30, ValidReason);

        var raisedEvent = Assert.Single(user.UncommittedDomainEvents);
        var pointsEvent = Assert.IsType<ClanPointsChangedEvent>(raisedEvent);
        Assert.Equal(TestUsers.DefaultActingUserId, pointsEvent.ActingUserId);
        Assert.Equal(user.Id, pointsEvent.UserId);
        Assert.Equal(-30, pointsEvent.Delta);
        Assert.Equal(ValidReason, pointsEvent.Reason);
    }

    [Fact]
    public void DeductClanPoints_AfterAddAndDeduct_EnforcesRunningTotalAgainstNetValue()
    {
        var user = TestUsers.CreateValidUser();
        user.AddClanPoints(TestUsers.DefaultActingUserId, 100, ValidReason);
        user.DeductClanPoints(TestUsers.DefaultActingUserId, 60, ValidReason);
        // Running total is now 40 (100 - 60), not 100 - deducting 41 must be rejected.

        Assert.Throws<DomainException>(() => user.DeductClanPoints(TestUsers.DefaultActingUserId, 41, ValidReason));

        user.DeductClanPoints(TestUsers.DefaultActingUserId, 40, ValidReason);
        Assert.Throws<DomainException>(() => user.DeductClanPoints(TestUsers.DefaultActingUserId, 1, ValidReason));
    }
}
