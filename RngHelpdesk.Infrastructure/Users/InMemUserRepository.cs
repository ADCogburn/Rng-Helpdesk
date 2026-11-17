using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Users;

public sealed class InMemUserRepository : IUserRepository
{
    private readonly Dictionary<ulong, List<IDomainEvent>> _events = new();

    public Task<bool> ExistsAsync(ulong userId, CancellationToken ct = default) => Task.FromResult(_events.ContainsKey(userId));

    public Task<bool> HasAnyUsersAsync(CancellationToken ct = default) => Task.FromResult(_events.Count > 0);

    public Task<User> GetByIdAsync(ulong userId, CancellationToken ct = default)
    {
        if (!_events.TryGetValue(userId, out var events))
            throw new AggregateNotFoundException(nameof(User), userId);

        // This in-memory store only ever holds IDomainEvents (see SaveAsync below), so the stream
        // version and the domain event count are always the same here.
        return Task.FromResult(User.Rehydrate(events, streamVersion: events.Count));
    }

    public Task<IReadOnlyCollection<IDomainEvent>> SaveAsync(User user, CancellationToken ct = default)
    {
        if (!_events.TryGetValue(user.Id, out var stream))
        {
            stream = new List<IDomainEvent>();
            _events[user.Id] = stream;
        }

        var newEvents = user.UncommittedDomainEvents.ToArray();

        stream.AddRange(newEvents);

        user.ClearUncommittedDomainEvents();

        return Task.FromResult<IReadOnlyCollection<IDomainEvent>>(newEvents);
    }

    public void Seed(ulong userId, IEnumerable<IDomainEvent> events)
    {
        _events[userId] = new List<IDomainEvent>(events);
    }

    // The user ID is the Discord ID (set from DiscordAccount.DiscordId in UserCreatedEvent),
    // so an existence check by Discord ID is the same as checking the stream keyed on that ID.
    public Task<bool> UserExistsWithDiscordIdAsync(ulong discordId, CancellationToken ct = default) =>
        ExistsAsync(discordId, ct);

    public Task<bool> UserExistsWithDiscordUsernameAsync(string username, CancellationToken ct = default)
    {
        var exists = _events.Values
            .Select(stream => stream.OfType<UserCreatedEvent>().FirstOrDefault())
            .Where(e => e is not null)
            .Any(e => e!.DiscordAccount.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(exists);
    }
}