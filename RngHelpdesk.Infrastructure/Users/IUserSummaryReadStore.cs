namespace RngHelpdesk.Infrastructure.Users;

/// <summary>
/// Defines access to user(s) in a UserSummaryProjection. 
/// </summary>
public interface IUserSummaryReadStore
{
    Task<UserSummaryReadModel?> GetByIdAsync(ulong userId, CancellationToken ct = default);
    Task<UserSummaryReadModel?> GetByRunescapeUsernameAsync(string rsn, CancellationToken ct = default);
    Task<IReadOnlyCollection<UserSummaryReadModel>> GetAllAsync(CancellationToken ct = default);
}
