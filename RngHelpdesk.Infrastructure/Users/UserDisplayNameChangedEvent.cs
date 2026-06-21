using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Users;

public sealed record UserDisplayNameChangedEvent(
    ulong UserId,
    ulong ActingUserId,
    string NewDisplayName,
    DateTimeOffset OccurredAt) : IApplicationEvent;