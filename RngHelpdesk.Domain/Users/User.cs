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

        ClanPoints += points;

        return new PointsEvent(
            userId: Id,
            delta: points,
            reason: reason,
            occurredAt: DateTime.UtcNow
        );
    }

    public void RemoveClanPoints(int points)
    {
        if (points <= 0)
            throw new ArgumentException("Points to remove must be greater than zero.", nameof(points));
        if (points > ClanPoints)
            throw new InvalidOperationException("Cannot remove more points than the user currently has.");

        ClanPoints -= points;
    }

    public void LinkDiscordAccount(ulong discordId)
    {
        if (DiscordAccounts.Any(da => da.DiscordId == discordId))
            return;

        DiscordAccounts.Add(new DiscordAccount(discordId));
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
}
