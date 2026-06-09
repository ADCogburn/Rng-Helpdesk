using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Infrastructure.Users;

public interface IUserRepository
{
    User GetById(int userId);
    IReadOnlyCollection<IDomainEvent> Save(User user);
    bool Exists(int userId);
    bool HasAnyUsers();
    bool UserExistsWithDiscordId(ulong discordId);
    bool UserExistsWithDiscordUsername(string username);
}
