using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Infrastructure.Users;

public interface IUserLifecycleHistoryReadStore
{
    IReadOnlyList<UserLifecycleHistoryItem> GetLifecycleHistoryForUserById(ulong userId)
}
