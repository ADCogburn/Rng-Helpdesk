namespace RngHelpdesk.Contracts.Users.Queries;

public sealed class GetUserLifecycleHistoryQuery
{
    public ulong UserId { get; init; }
}