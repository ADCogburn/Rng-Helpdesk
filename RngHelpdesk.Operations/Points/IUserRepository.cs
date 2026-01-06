using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Points;

public interface IUserRepository
{
    User GetById(int userId);
    void Save(User user);
}
