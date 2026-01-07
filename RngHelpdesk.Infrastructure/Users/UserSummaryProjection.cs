using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Users;

public sealed class UserSummaryProjection :
    IProjectionHandler<UserCreatedEvent>,
    IProjectionHandler<UserDeactivatedEvent>,
    IProjectionHandler<UserReactivatedEvent>,
    IProjectionHandler<ClanPointsChangedEvent>,
    IProjectionHandler<AuthorityRoleChangedEvent>,
    IProjectionHandler<DiscordAccountLinkedEvent>,
    IProjectionHandler<DiscordAccountDelinkedEvent>,
    IProjectionHandler<RunescapeAccountLinkedEvent>,
    IProjectionHandler<RunescapeAccountDelinkedEvent>,
    IProjectionHandler<RunescapeAccountRenamedEvent>
{
    private readonly Dictionary<int, UserSummaryReadModel> _users = new();

    public IReadOnlyCollection<UserSummaryReadModel> GetAll() => _users.Values;

    public UserSummaryReadModel GetSingleById(int userId)
    {
        if (!_users.TryGetValue(userId, out var user))
            throw new InvalidOperationException($"User {userId} not found.");

        return user;
    }

    public UserSummaryReadModel GetByDiscordId(ulong discordId)
    {
        var user = _users.Values.FirstOrDefault(u =>
            u.DiscordAccounts.Any(d => d.DiscordId == discordId));

        if (user is null)
            throw new InvalidOperationException(
                $"No user linked to DiscordId {discordId}");

        return user;
    }

    public UserSummaryReadModel GetByRunescapeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username must be provided.", nameof(username));

        var user = _users.Values.FirstOrDefault(u =>
            u.RunescapeAccounts.Any(r =>
                r.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));

        if (user is null)
            throw new InvalidOperationException(
                $"No user linked to Runescape account '{username}'");

        return user;
    }

    #region Projections

    public void Project(UserCreatedEvent e)
    {
        _users[e.UserId] = new UserSummaryReadModel
        {
            UserId = e.UserId,
            AuthorityRole = e.AuthorityRole,
            IsActive = true,
            DateCreated = e.OccurredAt,
            RunescapeAccounts = e.RunescapeAccounts
                .Select(a => new RunescapeAccountView
                {
                    Username = a.Username
                })
                .ToList(),
            DiscordAccounts = e.DiscordAccounts
                .Select(a => new DiscordAccountView
                {
                    DiscordId = a.DiscordId,
                    Username = a.Username,
                    IsActive = a.IsActive
                })
                .ToList()
        };
    }

    public void Project(UserDeactivatedEvent e)
    {
        var existing = _users[e.UserId];

        _users[e.UserId] = existing with
        {
            IsActive = false
        };
    }

    public void Project(UserReactivatedEvent e)
    {
        var existing = _users[e.UserId];

        _users[e.UserId] = existing with
        {
            IsActive = true
        };
    }

    public void Project(ClanPointsChangedEvent e)
    {
        var existing = _users[e.UserId];

        _users[e.UserId] = existing with
        {
            ClanPoints = existing.ClanPoints + e.Delta
        };
    }

    public void Project(AuthorityRoleChangedEvent e)
    {
        var existing = _users[e.UserId];
        _users[e.UserId] = existing with
        {
            AuthorityRole = e.NewRole
        };
    }

    public void Project(DiscordAccountLinkedEvent e)
    {
        var existing = _users[e.UserId];

        var updatedAccounts = existing.DiscordAccounts
            .Append(new DiscordAccountView
            {
                DiscordId = e.DiscordId,
                Username = e.Username,
                IsActive = true
            })
            .ToList();

        _users[e.UserId] = existing with
        {
            DiscordAccounts = updatedAccounts
        };
    }

    public void Project(DiscordAccountDelinkedEvent e)
    {
        var existing = _users[e.UserId];

        var updatedAccounts = existing.DiscordAccounts
            .Where(a => a.DiscordId != e.DiscordId)
            .ToList();

        _users[e.UserId] = existing with
        {
            DiscordAccounts = updatedAccounts
        };
    }

    public void Project(RunescapeAccountLinkedEvent e)
    {
        var existing = _users[e.UserId];

        var updatedAccounts = existing.RunescapeAccounts
            .Append(new RunescapeAccountView
            {
                Username = e.Username
            })
            .ToList();

        _users[e.UserId] = existing with
        {
            RunescapeAccounts = updatedAccounts
        };
    }

    public void Project(RunescapeAccountDelinkedEvent e)
    {
        var existing = _users[e.UserId];

        var updatedAccounts = existing.RunescapeAccounts
            .Where(a =>
                !a.Username.Equals(e.Username, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _users[e.UserId] = existing with
        {
            RunescapeAccounts = updatedAccounts
        };
    }

    public void Project(RunescapeAccountRenamedEvent e)
    {
        var existing = _users[e.UserId];

        var updatedAccounts = existing.RunescapeAccounts
            .Where(a => !a.Username.Equals(e.OldUsername, StringComparison.OrdinalIgnoreCase))
            .Append(new RunescapeAccountView
            {
                Username = e.NewUsername
            })
            .ToList();

        _users[e.UserId] = existing with
        {
            RunescapeAccounts = updatedAccounts
        };
    }

    #endregion
}