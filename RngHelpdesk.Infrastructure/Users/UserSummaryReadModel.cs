using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Infrastructure.Users;

public sealed record UserSummaryReadModel
{
    public int UserId { get; init; }
    public AuthorityRole AuthorityRole { get; init; }
    public bool IsActive { get; init; }
    public DateTime DateCreated { get; init; }
    public IReadOnlyList<RunescapeAccountView> RunescapeAccounts { get; init; } = [];
    public IReadOnlyList<DiscordAccountView> DiscordAccounts { get; init; } = [];
}