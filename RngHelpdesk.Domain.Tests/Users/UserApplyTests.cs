using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Domain.Tests.Users;

public class UserApplyTests
{
    private sealed class UnrecognizedDomainEvent : IDomainEvent
    {
        public ulong ActingUserId => TestUsers.DefaultActingUserId;
        public DateTimeOffset OccurredAt => DateTimeOffset.UtcNow;
    }

    [Fact]
    public void Rehydrate_UnrecognizedEventType_ThrowsDomainException()
    {
        var events = new IDomainEvent[] { new UnrecognizedDomainEvent() };

        Assert.Throws<DomainException>(() => User.Rehydrate(events));
    }
}
