namespace RngHelpdesk.Contracts.Users.Commands;

public class DelinkRunescapeAccountRequest
{
    public int UserId { get; init; }
    public string Username { get; init; } = default!;
}
