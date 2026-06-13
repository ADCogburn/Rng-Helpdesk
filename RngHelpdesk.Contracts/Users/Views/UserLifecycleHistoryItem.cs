namespace RngHelpdesk.Contracts.Users.Views;

public sealed class UserLifecycleHistoryItem
{
    public string Action { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
}