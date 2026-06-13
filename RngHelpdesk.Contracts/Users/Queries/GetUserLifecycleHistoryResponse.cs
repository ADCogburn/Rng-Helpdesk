using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Contracts.Users.Queries;

public sealed class GetUserLifecycleHistoryResponse
{
    public ulong UserId { get; init; }
    public IReadOnlyList<UserLifecycleHistoryItem> History { get; init; } = [];
}