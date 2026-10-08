using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Users.RunescapeAccount;

public sealed class RunescapeAccountHistoryProjection :
    IProjectionState,
    IProjectionHandler<RunescapeAccountLinkedEvent>,
    IProjectionHandler<RunescapeAccountRenamedEvent>,
    IProjectionHandler<RunescapeAccountDelinkedEvent>,
    IRunescapeAccountHistoryReadStore
{
    // Instead of looping through all users searching for a userId/username, simply use these caches.

    private readonly Dictionary<ulong, List<RunescapeAccountHistoryItem>> _history = new();

    private readonly Dictionary<string, HashSet<ulong>> _historicalUsernameIndex = new(StringComparer.OrdinalIgnoreCase); // Match a given RSN to all Users who have used it before (inclusive to currently using it).
    private readonly Dictionary<ulong, HashSet<string>> _previousUsernames = new(); // Match a given UserId to all RSNs they have ever used before excluding those they are currently using.
    private readonly object _lock = new();

    public bool IsEmpty
    {
        get { lock (_lock) { return _history.Count == 0; } }
    }

    public Task<IReadOnlyList<RunescapeAccountView>> GetPreviousRunescapeAccountsAsync(ulong userId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<RunescapeAccountView>>(_previousUsernames.TryGetValue(userId, out var accounts)
                ? accounts.Select(a => new RunescapeAccountView(a)).ToList()
                : []);
        }
    }

    public Task<IReadOnlyList<RunescapeAccountHistoryItem>> GetHistoryAsync(ulong userId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<RunescapeAccountHistoryItem>>(_history.TryGetValue(userId, out var list)
                ? list.ToList()
                : Array.Empty<RunescapeAccountHistoryItem>());
        }
    }

    public Task<IReadOnlyCollection<ulong>?> GetUserIdsByHistoricalRunescapeUsernameAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username must be provided.", nameof(username));

        lock (_lock)
        {
            if (!_historicalUsernameIndex.TryGetValue(username, out var foundUserIds))
                return Task.FromResult<IReadOnlyCollection<ulong>?>(null);

            return Task.FromResult<IReadOnlyCollection<ulong>?>(foundUserIds.ToList());
        }
    }

    #region Projections

    public void Project(RunescapeAccountLinkedEvent e)
    {
        lock (_lock)
        {
            Add(e.UserId, new RunescapeAccountHistoryItem
            {
                ChangeType = RunescapeAccountChangeType.Linked,
                Username = e.Username,
                OccurredAt = e.OccurredAt
            });

            RemovePrevious(e.UserId, e.Username);

            Index(e.Username, e.UserId);
        }
    }

    public void Project(RunescapeAccountRenamedEvent e)
    {
        lock (_lock)
        {
            Add(e.UserId, new RunescapeAccountHistoryItem
            {
                ChangeType = RunescapeAccountChangeType.Renamed,
                OldUsername = e.OldUsername,
                NewUsername = e.NewUsername,
                OccurredAt = e.OccurredAt
            });

            AddPrevious(e.UserId, e.OldUsername);
            RemovePrevious(e.UserId, e.NewUsername);

            Index(e.OldUsername, e.UserId);
            Index(e.NewUsername, e.UserId);
        }
    }

    public void Project(RunescapeAccountDelinkedEvent e)
    {
        lock (_lock)
        {
            Add(e.UserId, new RunescapeAccountHistoryItem
            {
                ChangeType = RunescapeAccountChangeType.Delinked,
                Username = e.Username,
                OccurredAt = e.OccurredAt
            });

            AddPrevious(e.UserId, e.Username);

            Index(e.Username, e.UserId);
        }
    }

    private void Add(ulong userId, RunescapeAccountHistoryItem item)
    {
        if (!_history.TryGetValue(userId, out var list))
        {
            list = new List<RunescapeAccountHistoryItem>();
            _history[userId] = list;
        }

        list.Add(item);
    }

    /// <summary>
    /// Adds this RSN to the "previous usernames" list.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="username"></param>
    private void AddPrevious(ulong userId, string username)
    {
        if (!_previousUsernames.TryGetValue(userId, out var accounts))
        {
            accounts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _previousUsernames[userId] = accounts;
        }

        accounts.Add(username);
    }

    /// <summary>
    /// Removes this RSN from the "previous usernames" list.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="username"></param>
    private void RemovePrevious(ulong userId, string username)
    {
        if (_previousUsernames.TryGetValue(userId, out var accounts))
            accounts.Remove(username);
    }

    private void Index(string username, ulong userId)
    {
        if (!_historicalUsernameIndex.TryGetValue(username, out var userIds))
        {
            userIds = new HashSet<ulong>();
            _historicalUsernameIndex[username] = userIds;
        }

        userIds.Add(userId);
    }

    #endregion
}