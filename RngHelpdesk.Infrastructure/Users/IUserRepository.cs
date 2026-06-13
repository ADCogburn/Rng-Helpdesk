using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Infrastructure.Users;

public interface IUserRepository
{
    User GetById(ulong userId);
    IReadOnlyCollection<IDomainEvent> Save(User user);
    bool Exists(ulong userId);
    bool HasAnyUsers();
    bool UserExistsWithDiscordId(ulong discordId);
    bool UserExistsWithDiscordUsername(string username);
}
