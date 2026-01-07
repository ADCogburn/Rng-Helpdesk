namespace RngHelpdesk.Contracts.Users.Views;

public sealed class RunescapeAccountHistoryItem
{
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// True if this is currently linked to the user.
    /// </summary>
    public bool IsCurrent { get; init; }

    /// <summary>
    /// True if this username was delinked.
    /// </summary>
    public bool IsDelinked { get; init; }

    /// <summary>
    /// True if this username was replaced by a rename.
    /// </summary>
    public bool IsPreviousName { get; init; }

    public DateTime OccurredAt { get; init; }
}

