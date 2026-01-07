namespace RngHelpdesk.Contracts.Users.Commands;

public sealed class RenameRunescapeAccountRequest
{
    public int UserId { get; init; }

    public string OldUsername { get; init; } = string.Empty;

    public string NewUsername { get; init; } = string.Empty;
}