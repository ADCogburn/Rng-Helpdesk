using RngHelpdesk.Contracts.Models.Users.Dtos;

namespace RngHelpdesk.Contracts.Users.Commands;

public sealed class CreateUserRequest
{
    public DiscordAccountDto DiscordAccount { get; init; } = default!;

    public IReadOnlyList<RunescapeAccountDto> RunescapeAccounts { get; init; } = [];
}