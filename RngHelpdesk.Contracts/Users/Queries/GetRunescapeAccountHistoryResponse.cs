using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Contracts.Users.Queries;

public sealed class GetRunescapeAccountHistoryResponse
{
    public int UserId { get; init; }

    public IReadOnlyList<RunescapeAccountHistoryItem> History { get; init; } = [];
}