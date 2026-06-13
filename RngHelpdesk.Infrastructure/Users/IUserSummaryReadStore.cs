namespace RngHelpdesk.Infrastructure.Users;

/// <summary>
/// Defines access to user(s) in a UserSummaryProjection. 
/// </summary>
public interface IUserSummaryReadStore
{
    bool TryGetById(ulong userId, out UserSummaryReadModel? user);
    bool TryGetByRunescapeUsername(string rsn, out UserSummaryReadModel? user);
    IReadOnlyCollection<UserSummaryReadModel> GetAll();
}
