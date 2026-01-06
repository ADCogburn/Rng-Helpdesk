using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Points;

public class InMemUserRepository : IUserRepository
{
    private readonly Dictionary<int, List<IDomainEvent>> _events = new();

    public User GetById(int userId)
    {
        var user = new User();

        if (_events.TryGetValue(userId, out var history))
        {
            user.LoadFromHistory(history);
        }

        return user;
    }

    public void Save(User user)
    {
        if (!_events.TryGetValue(user.Id, out var history))
        {
            history = new List<IDomainEvent>();
            _events[user.Id] = history;
        }

        history.AddRange(user.UncommittedDomainEvents);
        user.ClearUncommittedDomainEvents();
    }
}
