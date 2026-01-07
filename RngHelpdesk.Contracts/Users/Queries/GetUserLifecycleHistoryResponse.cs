using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Contracts.Users.Queries;

public sealed class GetUserLifecycleHistoryResponse
{
    public int UserId { get; init; }
    public IReadOnlyList<UserLifecycleHistoryItem> History { get; init; } = [];
}