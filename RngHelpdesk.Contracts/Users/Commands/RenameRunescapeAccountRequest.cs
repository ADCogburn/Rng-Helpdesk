namespace RngHelpdesk.Contracts.Users.Commands;

public sealed record RenameRunescapeAccountRequest(
    ulong ActingUserId,
    ulong UserId,
    string OldUsername,
    string NewUsername);