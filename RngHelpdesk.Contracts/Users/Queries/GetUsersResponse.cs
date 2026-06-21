using RngHelpdesk.Contracts.Models.Users;

namespace RngHelpdesk.Contracts.Users.Queries;

public sealed record GetUsersByHistoricalRunescapeUsernameQuery(string HistoricalRunescapeUsername);

public sealed record GetUsersResponse(IReadOnlyCollection<UserDto> Users);
