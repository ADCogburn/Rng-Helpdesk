using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users.Events;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RngHelpdesk.Infrastructure")] // IMPROVE this is used to keep the rehydration User internal, but allow other projects to access it.
                                                             // this should be changed somehow - maybe implement a factory pattern for rehydration instead.

namespace RngHelpdesk.Domain.Users;

public sealed class User : AggregateRoot
{
    public int Id { get; private set; }

    // ** Private Backing Fields **
    private readonly List<DiscordAccount> _discordAccounts = new();
    private readonly List<RunescapeAccount> _runescapeAccounts = new();


    // ** Public Read Models **
    public IReadOnlyCollection<DiscordAccount> DiscordAccounts => _discordAccounts;
    public IReadOnlyCollection<RunescapeAccount> RunescapeAccounts => _runescapeAccounts;


    // ** Non-Collection Models **
    private int _currentClanPoints; // private field used as a cache when loading from history.
    public DateTimeOffset DateCreated { get; private set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; private set; } = true;

    internal User()
    {
        // For rehydrating from events.
    }

    public static User Create(
        int userId,
        int actingUserId,
        IEnumerable<DiscordAccount> discordAccounts,
        IEnumerable<RunescapeAccount> runescapeAccounts)
    {
        var user = new User();

        user.RaiseDomainEvent(new UserCreatedEvent(
            userId,
            actingUserId,
            discordAccounts,
            runescapeAccounts,
            DateTimeOffset.UtcNow));

        return user;
    }

    /// <summary>
    /// This functionally just calls Apply (below) on all of the DomainEvents. However, it is particularly
    /// used for recreating the aggregate User from scratch without having to do for loops everywhere.
    /// </summary>
    /// <param name="events"></param>
    /// <returns></returns>
    public static User Rehydrate(IEnumerable<IDomainEvent> events)
    {
        var user = new User();
        user.LoadFromHistory(events);
        return user;
    }

    protected override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case UserCreatedEvent e:
                Id = e.UserId;
                IsActive = true;
                DateCreated = e.OccurredAt;
                _discordAccounts.AddRange(e.DiscordAccounts);
                _runescapeAccounts.AddRange(e.RunescapeAccounts);
                break;

            case UserDeactivatedEvent e:
                IsActive = false;
                break;

            case UserReactivatedEvent e:
                IsActive = true;
                break;

            case ClanPointsChangedEvent e:
                _currentClanPoints += e.Delta;
                break;

            case DiscordAccountLinkedEvent e:
                _discordAccounts.Add(new DiscordAccount(
                    e.DiscordId,
                    e.Username));
                break;

            case DiscordAccountDelinkedEvent e:
                _discordAccounts.RemoveAll(x => x.DiscordId == e.DiscordId);
                break;

            case RunescapeAccountLinkedEvent e:
                _runescapeAccounts.Add(new RunescapeAccount(e.Username));
                break;

            case RunescapeAccountDelinkedEvent e:
                _runescapeAccounts.RemoveAll(x => x.Username.Equals(e.Username, StringComparison.OrdinalIgnoreCase));
                break;

            case RunescapeAccountRenamedEvent e:
                _runescapeAccounts.RemoveAll(x => x.Username.Equals(e.OldUsername, StringComparison.OrdinalIgnoreCase));
                _runescapeAccounts.Add(new RunescapeAccount(e.NewUsername));
                break;

            default:
                throw new DomainException(
                    $"User aggregate cannot apply event type {domainEvent.GetType().Name}");
        }
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        RaiseDomainEvent(new UserDeactivatedEvent(Id));
    }

    public void Reactivate()
    {
        if (IsActive)
            return;

        RaiseDomainEvent(new UserReactivatedEvent(Id));
    }

    public void AddClanPoints(int points, string reason)
    {
        if (points <= 0)
            throw new ArgumentException("Points to add must be greater than zero.", nameof(points));

        if (reason is null || reason == string.Empty)
            throw new ArgumentException("Reason for adding points must be provided.", nameof(reason));

        RaiseDomainEvent(new ClanPointsChangedEvent(Id, points, reason));
    }

    public void DeductClanPoints(int points, string reason)
    {
        if (points <= 0)
            throw new ArgumentException("Points to remove must be greater than zero.", nameof(points));

        if (_currentClanPoints - points < 0)
            throw new DomainException("Cannot deduct clan points below zero.");

        RaiseDomainEvent(new ClanPointsChangedEvent(Id, -points, reason));
    }

    public void AddDiscordAccount(ulong discordId, string username)
    {
        if (_discordAccounts.Any(a => a.DiscordId == discordId))
            throw new DomainException("Discord account already linked.");

        RaiseDomainEvent(new DiscordAccountLinkedEvent(
            Id,
            discordId,
            username));
    }

    public void RemoveDiscordAccount(ulong discordId)
    {
        var account = _discordAccounts.FirstOrDefault(a => a.DiscordId == discordId);

        if (account is null)
            throw new DomainException("Discord account not linked.");

        RaiseDomainEvent(new DiscordAccountDelinkedEvent(
            Id,
            discordId));
    }

    public void AddRunescapeAccount(string username)
    {
        if (_runescapeAccounts.Any(a =>
            a.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("Runescape account already linked.");
        }

        RaiseDomainEvent(new RunescapeAccountLinkedEvent(
            Id,
            username));
    }

    public void RenameRunescapeAccount(string oldUsername, string newUsername)
    {
        var account = _runescapeAccounts.FirstOrDefault(a =>
            a.Username.Equals(oldUsername, StringComparison.OrdinalIgnoreCase));

        if (account is null)
            throw new DomainException("Runescape account not found.");

        if (_runescapeAccounts.Any(a =>
            a.Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Runescape account already exists.");

        RaiseDomainEvent(new RunescapeAccountRenamedEvent(
            Id,
            oldUsername,
            newUsername));
    }

    public void RemoveRunescapeAccount(string username)
    {
        var account = _runescapeAccounts.FirstOrDefault(a =>
            a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (account is null)
            throw new DomainException("Runescape account not linked.");

        RaiseDomainEvent(new RunescapeAccountDelinkedEvent(
            Id,
            username));
    }
}