namespace RngHelpdesk.Contracts.Users.Queries;

public sealed record GetUsersByHistoricalRunescapeUsernameQuery(string HistoricalRunescapeUsername);

public sealed record GetUsersResponse(IReadOnlyCollection<GetUserResponse> Users);
