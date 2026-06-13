namespace RngHelpdesk.Contracts.Users.Views;

public sealed class RunescapeAccountHistoryItem
{
    public RunescapeAccountChangeType ChangeType { get; init; }

    public string? Username { get; init; }
    public string? OldUsername { get; init; }
    public string? NewUsername { get; init; }

    public DateTimeOffset OccurredAt { get; init; }
}

public enum RunescapeAccountChangeType
{
    Linked,
    Renamed,
    Delinked
}