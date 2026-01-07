namespace RngHelpdesk.Contracts.Users.Queries;

public sealed class GetAllUsersResponse
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<GetUserResponse> Users { get; init; } = [];
}
