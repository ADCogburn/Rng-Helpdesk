namespace RngHelpdesk.Contracts.Users.Commands;

public sealed class CreateUserResponse
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string TemporaryPassword { get; init; } = string.Empty;
}
