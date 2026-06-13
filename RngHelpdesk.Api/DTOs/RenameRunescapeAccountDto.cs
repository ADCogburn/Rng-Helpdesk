namespace RngHelpdesk.Api.DTOs;

public sealed record RenameRunescapeAccountDto(
    string OldUsername,
    string NewUsername);
