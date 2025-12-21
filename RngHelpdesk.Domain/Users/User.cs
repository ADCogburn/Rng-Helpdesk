namespace RngHelpdesk.Domain.Users;

public sealed class User
{
    public int Id { get; set; }
    public List<DiscordAccount> DiscordAccounts { get; } = new();
    public List<RunescapeAccount> RunescapeAccounts { get; } = new();
    public List<RunescapeAccount> PreviousRunescapeAccounts { get; } = new();
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

        DiscordAccounts = new List<DiscordAccount>(discordAccounts);
        RunescapeAccounts = new List<RunescapeAccount>(runescapeAccounts);
    }



    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public PointsEvent AddClanPoints(int points, string reason)
    {
        if (points <= 0)
            throw new ArgumentException("Points to add must be greater than zero.", nameof(points));

        if (reason is null || reason == string.Empty)
            throw new ArgumentException("Reason for adding points must be provided.", nameof(reason));

        ClanPoints += points;

        return new PointsEvent(
            userId: Id,
            delta: points,
            reason: reason,
            occurredAt: DateTime.UtcNow
        );
    }

    public void DeductClanPoints(int points)
    {
        if (points <= 0)
            throw new ArgumentException("Points to remove must be greater than zero.", nameof(points));

        if (ClanPoints - points < 0)
            ClanPoints = 0;
        else
            ClanPoints -= points;
    }

    public void AddDiscordAccount(ulong discordId, string username)
    {
        if (DiscordAccounts.Any(da => da.DiscordId == discordId))
            return;

        DiscordAccounts.Add(new DiscordAccount(discordId, username));
    }

    public void RemoveDiscordAccount(ulong discordId)
    {
        var account = DiscordAccounts.FirstOrDefault(da => da.DiscordId == discordId);
        if (account != null)
        {
            DiscordAccounts.Remove(account);
        }
    }

    public void AddRunescapeAccount(string username)
    {
        if (RunescapeAccounts.Any(ra => ra.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            return;

        RunescapeAccounts.Add(new RunescapeAccount(username));
    }

    public void RemoveRunescapeAccount(string username)
    {
        var account = RunescapeAccounts.FirstOrDefault(ra => ra.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (account != null)
        {
            RunescapeAccounts.Remove(account);
            PreviousRunescapeAccounts.Add(account);
        }
    }

    public void ChangeRunescapeUsername(string currentUsername, string newUsername)
    {
        var account = RunescapeAccounts.FirstOrDefault(ra => ra.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase));
        if (account == null)
            throw new ArgumentException("Runescape account not found.", nameof(currentUsername));

        RunescapeAccounts.Remove(account);
        PreviousRunescapeAccounts.Add(account);

        var newAccount = new RunescapeAccount(newUsername);
        RunescapeAccounts.Add(newAccount);
    }
}
