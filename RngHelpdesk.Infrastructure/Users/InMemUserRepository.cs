using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Infrastructure.Users;

public class InMemUserRepository : IUserRepository
{
    private readonly Dictionary<int, User> _users = new();
    private readonly Dictionary<int, List<IDomainEvent>> _events = new();

    public User GetById(int userId)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            throw new InvalidOperationException($"User {userId} not found.");
        }

        // Rehydrate event-sourced state
        if (_events.TryGetValue(userId, out var history))
        {
            user.LoadFromHistory(history);
        }

        return user;
    }

    public IReadOnlyCollection<IDomainEvent> Save(User user)
    {
        if (!_users.ContainsKey(user.Id))
        {
            _users[user.Id] = user;
        }

        if (!_events.TryGetValue(user.Id, out var history))
        {
            history = new List<IDomainEvent>();
            _events[user.Id] = history;
        }

        history.AddRange(user.UncommittedDomainEvents);

        var events = user.UncommittedDomainEvents.ToList();

        user.ClearUncommittedDomainEvents();

        return events;
    }

    public bool Exists(int userId) => _events.ContainsKey(userId);

    public void Seed(int userId, IEnumerable<IDomainEvent> events)
    {
        _events[userId] = new List<IDomainEvent>(events);
    }
}
