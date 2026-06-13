using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Users;

public sealed class InMemUserRepository : IUserRepository
{
    private readonly Dictionary<ulong, List<IDomainEvent>> _events = new();

    public bool Exists(ulong userId) => _events.ContainsKey(userId);

    public bool HasAnyUsers() => _events.Count > 0;

    public User GetById(ulong userId)
    {
        if (!_events.TryGetValue(userId, out var events))
            throw new AggregateNotFoundException(nameof(User), userId);

        return User.Rehydrate(events);
    }

    public IReadOnlyCollection<IDomainEvent> Save(User user)
    {
        if (!_events.TryGetValue(user.Id, out var stream))
        {
            stream = new List<IDomainEvent>();
            _events[user.Id] = stream;
        }

        var newEvents = user.UncommittedDomainEvents.ToArray();

        stream.AddRange(newEvents);

        user.ClearUncommittedDomainEvents();

        return newEvents;
    }

    public void Seed(ulong userId, IEnumerable<IDomainEvent> events)
    {
        _events[userId] = new List<IDomainEvent>(events);
    }

    public bool UserExistsWithDiscordId(ulong discordId)
    {
        throw new NotImplementedException();
    }

    public bool UserExistsWithDiscordUsername(string username)
    {
        throw new NotImplementedException();
    }
}