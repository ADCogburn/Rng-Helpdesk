namespace RngHelpdesk.Contracts.Users.Commands;

public sealed record DeactivateUserRequest(
    ulong ActingUserId,
    ulong UserId);
