namespace RngHelpdesk.Contracts.Users.Queries;

public sealed class GetRunescapeAccountsResponse
{
    public IReadOnlyList<string> Accounts { get; init; } = [];
}
