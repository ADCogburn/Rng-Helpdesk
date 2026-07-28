using RngHelpdesk.Contracts.Points.Views;

namespace RngHelpdesk.Infrastructure.Points;

public interface IPointHistoryReadStore
{
    Task<IReadOnlyList<PointHistoryItem>> GetPointHistoryForUserAsync(ulong userId, CancellationToken ct = default);
    Task<int> GetCountForUserAsync(ulong userId, CancellationToken ct = default);
}
