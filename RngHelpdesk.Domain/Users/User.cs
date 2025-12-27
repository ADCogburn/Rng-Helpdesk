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


    // ** Public Models With Private Setters **
    public int ClanPoints { get; private set; }
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
        ClanPoints = 0;
        IsActive = true;

        _discordAccounts = new List<DiscordAccount>(discordAccounts);
        _runescapeAccounts = new List<RunescapeAccount>(runescapeAccounts);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void AddClanPoints(int points, string reason)
    {
        if (points <= 0)
            throw new ArgumentException("Points to add must be greater than zero.", nameof(points));

        if (reason is null || reason == string.Empty)
            throw new ArgumentException("Reason for adding points must be provided.", nameof(reason));

        ClanPoints += points;

        RaiseDomainEvent(new ClanPointsChangedEvent(Id, points, reason, DateTime.UtcNow));
    }

    public void DeductClanPoints(int points, string reason)
    {
        if (points <= 0)
            throw new ArgumentException("Points to remove must be greater than zero.", nameof(points));

        // IMPROVE: Handle reduction of points below zero to just hit/stay at zero, while still maintaining the domain event below correctly.
        ClanPoints -= points;

        RaiseDomainEvent(new ClanPointsChangedEvent(Id, -points, reason, DateTime.UtcNow));
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
