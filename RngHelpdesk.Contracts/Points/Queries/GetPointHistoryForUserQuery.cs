namespace RngHelpdesk.Contracts.Points.Queries;

public sealed class GetPointHistoryForUserQuery
{
    public ulong UserId { get; init; }
}
