namespace RngHelpdesk.Contracts.Users.Queries;

public sealed class GetRunescapeAccountsQuery
{
    public required ulong UserId { get; init; }
}