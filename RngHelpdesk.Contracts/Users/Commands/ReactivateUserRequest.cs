namespace RngHelpdesk.Contracts.Users.Commands;

public sealed record ReactivateUserRequest(
    ulong ActingUserId,
    ulong UserId);
