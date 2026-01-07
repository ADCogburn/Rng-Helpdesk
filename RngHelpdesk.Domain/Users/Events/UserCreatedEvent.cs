using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class UserCreatedEvent : IDomainEvent
{
    public int UserId { get; }
    public AuthorityRole AuthorityRole { get; }
    public IReadOnlyList<DiscordAccount> DiscordAccounts { get; }
    public IReadOnlyList<RunescapeAccount> RunescapeAccounts { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public UserCreatedEvent(
        int userId,
        AuthorityRole authorityRole,
        IEnumerable<DiscordAccount> discordAccounts,
        IEnumerable<RunescapeAccount> runescapeAccounts)
    {
        UserId = userId;
        AuthorityRole = authorityRole;
        DiscordAccounts = discordAccounts.ToList();
        RunescapeAccounts = runescapeAccounts.ToList();
    }
}
