using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Infrastructure.Users.RunescapeAccount;

public interface IRunescapeAccountHistoryReadStore
{
    IReadOnlyList<RunescapeAccountHistoryItem> GetHistory(ulong userId);
    IReadOnlyList<RunescapeAccountView> GetPreviousRunescapeAccounts(ulong userId);
    bool TryGetUserIdsByHistoricalRunescapeUsername(string username, out IReadOnlyCollection<ulong> userIds); // in case more than 1 user has had an RSN before
}
