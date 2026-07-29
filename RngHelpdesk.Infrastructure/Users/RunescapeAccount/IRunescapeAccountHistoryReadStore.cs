using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Infrastructure.Users.RunescapeAccount;

public interface IRunescapeAccountHistoryReadStore
{
    Task<IReadOnlyList<RunescapeAccountHistoryItem>> GetHistoryAsync(ulong userId, CancellationToken ct = default);
    Task<IReadOnlyList<RunescapeAccountView>> GetPreviousRunescapeAccountsAsync(ulong userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<ulong>?> GetUserIdsByHistoricalRunescapeUsernameAsync(string username, CancellationToken ct = default); // in case more than 1 user has had an RSN before
}
