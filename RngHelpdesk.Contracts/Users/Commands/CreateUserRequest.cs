using RngHelpdesk.Contracts.Models.Users.Dtos;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Contracts.Users.Commands;

public sealed class CreateUserRequest
{
    public int UserId { get; init; }

    public AuthorityRole AuthorityRole { get; init; } = AuthorityRole.Member;

    public IReadOnlyList<DiscordAccountDto> DiscordAccounts { get; init; } = [];

    public IReadOnlyList<RunescapeAccountDto> RunescapeAccounts { get; init; } = [];
}