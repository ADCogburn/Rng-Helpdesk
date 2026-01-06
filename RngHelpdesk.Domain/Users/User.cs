using RngHelpdesk.Domain.Common;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RngHelpdesk.Operations")] // IMPROVE this is used to keep the rehydration User internal, but allow other projects to access it.
                                                         // this should be changed somehow - maybe implement a factory pattern for rehydration instead.

namespace RngHelpdesk.Domain.Users;

public sealed class User : AggregateRoot
{
    public int Id { get; private set; }

    // ** Private Backing Fields **
    private readonly List<DiscordAccount> _discordAccounts = new();
    private readonly List<RunescapeAccount> _runescapeAccounts = new();
    private readonly List<RunescapeAccount> _previousRunescapeAccounts = new();


    // ** Public Read Models **
    public IReadOnlyCollection<DiscordAccount> DiscordAccounts => _discordAccounts;
    public IReadOnlyCollection<RunescapeAccount> RunescapeAccounts => _runescapeAccounts;
    public IReadOnlyCollection<RunescapeAccount> PreviousRunescapeAccounts => _previousRunescapeAccounts;


    // ** Non-Collection Models **
    private int _currentClanPoints; // private field used as a cache when loading from history.
    public AuthorityRole AuthorityRole { get; private set; } = AuthorityRole.Member;
    public DateTime DateCreated { get; private set; } = DateTime.UtcNow;
    public bool IsActive { get; private set; } = true;


    public User(
        int id,
        AuthorityRole role,
        IEnumerable<DiscordAccount> discordAccounts,
        IEnumerable<RunescapeAccount> runescapeAccounts)
    {
        Id = id;
        AuthorityRole = role;
        IsActive = true;

        _discordAccounts = new List<DiscordAccount>(discordAccounts);
        _runescapeAccounts = new List<RunescapeAccount>(runescapeAccounts);
    }

    internal User()
    {
        // For rehydrating from events.
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    protected override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case ClanPointsChangedEvent e:
                _currentClanPoints += e.Delta;
                break;

            default:
                throw new DomainException(
                    $"User aggregate cannot apply event type {domainEvent.GetType().Name}");
        }
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
        if (_discordAccounts.Any(da => da.DiscordId == discordId))
            return;

        _discordAccounts.Add(new DiscordAccount(discordId, username));
    }

    public void RemoveDiscordAccount(ulong discordId)
    {
        var account = _discordAccounts.FirstOrDefault(da => da.DiscordId == discordId);
        if (account != null)
        {
            _discordAccounts.Remove(account);
        }
    }

    public void AddRunescapeAccount(string username)
    {
        if (_runescapeAccounts.Any(ra => ra.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            return;

        _runescapeAccounts.Add(new RunescapeAccount(username));
    }

    public void RemoveRunescapeAccount(string username)
    {
        var account = _runescapeAccounts.FirstOrDefault(ra => ra.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (account != null)
        {
            _runescapeAccounts.Remove(account);
            _previousRunescapeAccounts.Add(account);
        }
    }

    public void ChangeRunescapeUsername(string currentUsername, string newUsername)
    {
        var account = _runescapeAccounts.FirstOrDefault(ra => ra.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase));
        if (account == null)
            throw new ArgumentException("Runescape account not found.", nameof(currentUsername));

        _runescapeAccounts.Remove(account);
        _runescapeAccounts.Add(account);

        var newAccount = new RunescapeAccount(newUsername);
        _runescapeAccounts.Add(newAccount);
    }

    public void ChangeAuthorityRole(AuthorityRole newRole)
    {
        AuthorityRole = newRole;
    }

}
