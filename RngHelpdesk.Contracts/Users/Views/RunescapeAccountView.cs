namespace RngHelpdesk.Contracts.Users.Views;

public sealed class RunescapeAccountView
{
    public string Username { get; init; } = default!;
    public bool IsActive { get; init; }
}
