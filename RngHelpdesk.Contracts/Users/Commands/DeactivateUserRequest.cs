namespace RngHelpdesk.Contracts.Users.Commands;

public sealed class DeactivateUserRequest
{
    public int UserId { get; init; }
}