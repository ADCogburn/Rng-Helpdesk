using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Points;

public sealed class AddPointsToUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEventDispatcher _eventDispatcher;

    public AddPointsToUserHandler(
        IUserRepository userRepository,
        IEventDispatcher eventdispatcher)
    {
        _userRepository = userRepository;
        _eventDispatcher = eventdispatcher;
    }

    public void Handle(
        IRequestContext context,
        AddPointsToUserRequest request)
    {
        AuthorizationRules.RequireRole(context, AuthorityRole.Administrator);

        var user = _userRepository.GetById(request.UserId);

        user.AddClanPoints(request.Points, request.Reason);

        var events = _userRepository.Save(user);

        _eventDispatcher.Dispatch(events);
    }
}
