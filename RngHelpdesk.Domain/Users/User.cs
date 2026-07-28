using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users.Events;

namespace RngHelpdesk.Domain.Users;

public sealed class User : AggregateRoot
{
    /// <summary>
    /// Unique identifier for the user. This is the same as the Discord Id.
    /// </summary>
    public ulong Id { get; private set; }

    // ** Private Backing Fields **
    private readonly List<RunescapeAccount> _runescapeAccounts = new();


    // ** Public Read Models **
    public DiscordAccount DiscordAccount { get; private set; } = default!;
    public IReadOnlyCollection<RunescapeAccount> RunescapeAccounts => _runescapeAccounts;


    // ** Non-Collection Models **
    private int _currentClanPoints; // private field used as a cache when loading from history.
    public DateTimeOffset DateCreated { get; private set; }
    public bool IsActive { get; private set; } = true;

    internal User()
    {
        // For rehydrating from events.
    }

    public static User Create(
        ulong actingUserId,
        DiscordAccount discordAccount,
        IEnumerable<RunescapeAccount> runescapeAccounts)
    {
        if (actingUserId == 0)
            throw new DomainException("Admin id required.");

        if (discordAccount == null || discordAccount.DiscordId == 0 || string.IsNullOrWhiteSpace(discordAccount.Username))
            throw new DomainException("Discord account, its snowflake Id, and its username are required.");

        var user = new User();

        user.RaiseDomainEvent(UserCreatedEvent.Create(
            actingUserId: actingUserId,
            discordAccount: discordAccount,
            runescapeAccounts: runescapeAccounts));

        return user;
    }

    /// <summary>
    /// Recreates the aggregate User from the events in the EventStore.
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
                DiscordAccount = e.DiscordAccount;
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

    public void Deactivate(ulong actingUserId)
    {
        if (actingUserId == 0)
            throw new DomainException("Admin id required.");

        if (!IsActive)
            throw new DomainException("User is already deactivated.");

        RaiseDomainEvent(UserDeactivatedEvent.Create(actingUserId: actingUserId, userId: Id));
    }

    public void Reactivate(ulong actingUserId)
    {
        if (actingUserId == 0)
            throw new DomainException("Admin id required.");

        if (IsActive)
            throw new DomainException("User is already activated.");

        RaiseDomainEvent(UserReactivatedEvent.Create(actingUserId: actingUserId, userId: Id));
    }

    public void AddClanPoints(ulong actingUserId, int points, string reason)
    {
        if (actingUserId == 0)
            throw new DomainException("Admin id required.");

        if (points <= 0)
            throw new DomainException("Points to add must be greater than zero.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Reason for adding points must be provided.");

        if (points > int.MaxValue - _currentClanPoints)
            throw new DomainException("Cannot add clan points above maximum limit.");

        RaiseDomainEvent(ClanPointsChangedEvent.Create(actingUserId: actingUserId, userId: Id, delta: points, reason: reason));
    }

    public void DeductClanPoints(ulong actingUserId, int points, string reason)
    {
        if (actingUserId == 0)
            throw new DomainException("Admin id required.");

        if (points <= 0)
            throw new DomainException("Points to remove must be greater than zero.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Reason for deducting points must be provided.");

        if (_currentClanPoints - points < 0)
            throw new DomainException("Cannot deduct clan points below zero.");

        RaiseDomainEvent(ClanPointsChangedEvent.Create(actingUserId: actingUserId, userId: Id, delta: -points, reason: reason));
    }

    public void AddRunescapeAccount(ulong actingUserId, string username)
    {
        if (actingUserId == 0)
            throw new DomainException("Admin id required.");

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new DomainException("Username cannot be empty.");
        }

        if (_runescapeAccounts.Any(a =>
            a.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("Runescape account already linked.");
        }

        RaiseDomainEvent(RunescapeAccountLinkedEvent.Create(actingUserId: actingUserId, userId: Id, username: username));
    }

    public void RenameRunescapeAccount(ulong actingUserId, string oldUsername, string newUsername)
    {
        if (actingUserId == 0)
            throw new DomainException("Admin id required.");

        if (string.IsNullOrWhiteSpace(oldUsername))
            throw new DomainException("Old username required.");

        if (string.IsNullOrWhiteSpace(newUsername))
            throw new DomainException("New username required.");

        var account = _runescapeAccounts.FirstOrDefault(a =>
            a.Username.Equals(oldUsername, StringComparison.OrdinalIgnoreCase));

        if (account is null)
            throw new DomainException("Runescape account not found.");

        if (_runescapeAccounts.Any(a =>
            a.Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Runescape account already exists.");

        RaiseDomainEvent(RunescapeAccountRenamedEvent.Create(actingUserId: actingUserId, userId: Id, oldUsername: account.Username, newUsername: newUsername));
    }

    public void RemoveRunescapeAccount(ulong actingUserId, string username)
    {
        if (actingUserId == 0)
            throw new DomainException("Admin id required.");

        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Runescape username required.");

        var account = _runescapeAccounts.FirstOrDefault(a =>
            a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (account is null)
            throw new DomainException("Runescape account not linked.");

        RaiseDomainEvent(RunescapeAccountDelinkedEvent.Create(actingUserId: actingUserId, userId: Id, username: account.Username));
    }
}