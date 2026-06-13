namespace RngHelpdesk.Contracts.Users.Commands;

public sealed record DelinkRunescapeAccountRequest(
    ulong ActingUserId,
    ulong UserId,
    string Username);
