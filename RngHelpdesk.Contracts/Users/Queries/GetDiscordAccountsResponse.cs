namespace RngHelpdesk.Contracts.Users.Queries;

public sealed class GetDiscordAccountsResponse
{
    public IReadOnlyCollection<ulong> DiscordIds { get; init; }
}
