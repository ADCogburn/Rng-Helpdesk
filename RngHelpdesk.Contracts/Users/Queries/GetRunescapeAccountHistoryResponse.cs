using RngHelpdesk.Contracts.Users.Views;

namespace RngHelpdesk.Contracts.Users.Queries;

public sealed record GetRunescapeAccountHistoryResponse(IReadOnlyList<RunescapeAccountHistoryItem> History);