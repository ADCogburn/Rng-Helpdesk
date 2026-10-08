using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Infrastructure.Users;

public interface IUserLifecycleHistoryReadStore
{
    Task<IReadOnlyList<UserLifecycleHistoryItem>> GetLifecycleHistoryForUserByIdAsync(ulong userId, CancellationToken ct = default);
}