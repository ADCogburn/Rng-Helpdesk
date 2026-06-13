namespace RngHelpdesk.Infrastructure.Security;

public sealed record AuthenticatedUser(ulong UserId, string Username, bool MustChangePassword);
