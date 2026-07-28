using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Infrastructure.Users;

public interface IUserRepository
{
    Task<User> GetByIdAsync(ulong userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<IDomainEvent>> SaveAsync(User user, CancellationToken ct = default);
    Task<bool> ExistsAsync(ulong userId, CancellationToken ct = default);
    Task<bool> HasAnyUsersAsync(CancellationToken ct = default);
    Task<bool> UserExistsWithDiscordIdAsync(ulong discordId, CancellationToken ct = default);
    Task<bool> UserExistsWithDiscordUsernameAsync(string username, CancellationToken ct = default);
}
