using RngHelpdesk.Contracts.Points.Views;

namespace RngHelpdesk.Infrastructure.Points;

public interface IPointHistoryReadStore
{
    IReadOnlyList<PointHistoryItem> GetPointHistoryForUser(ulong userId);
    int GetCountForUser(ulong userId);
}
