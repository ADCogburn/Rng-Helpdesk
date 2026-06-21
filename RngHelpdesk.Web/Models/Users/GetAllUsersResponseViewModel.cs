using RngHelpdesk.Contracts.Models.Users;

namespace RngHelpdesk.Web.Models.Users;

public sealed class GetAllUsersResponseViewModel
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<UserDto> Users { get; init; } = [];
}
