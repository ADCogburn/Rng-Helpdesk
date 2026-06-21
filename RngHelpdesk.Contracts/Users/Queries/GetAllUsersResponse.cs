using RngHelpdesk.Contracts.Models.Users;

namespace RngHelpdesk.Contracts.Users.Queries;

public sealed class GetAllUsersResponse
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<UserDto> Users { get; init; } = [];
}
