using RngHelpdesk.Contracts.Models.Users;

namespace RngHelpdesk.Contracts.Users.Queries;

// -- Queries for the "get user" request that returns user data --

public sealed record GetUserByIdQuery(ulong UserId);
public sealed record GetUserByRunescapeUsernameQuery(string RunescapeUsername);

// -- Returned promise data --

/// <summary>
/// The promise data of a user.
/// </summary>
public sealed record GetUserResponse(UserDto User);