using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Infrastructure.Security;

public sealed record AuthenticatedUser(int UserId, AppRole Role);
