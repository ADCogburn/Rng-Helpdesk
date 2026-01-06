using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Operations.Points;

public sealed class RemovePointsFromUserHandler
{

    private readonly IUserRepository _userRepository;
    private readonly IEventDispatcher _eventDispatcher;

    public RemovePointsFromUserHandler(
        IUserRepository userRepository,
        IEventDispatcher eventDispatcher)
    {
        _userRepository = userRepository;
        _eventDispatcher = eventDispatcher;
    }

    public void Handle(
        IRequestContext context,
        RemovePointsFromUserRequest request)
    {
        AuthorizationRules.RequireRole(context, AuthorityRole.Administrator);
        AuthorizationRules.RequireNonBot(context);

        var user = _userRepository.GetById(request.UserId);

        user.DeductClanPoints(request.Points, request.Reason);

        var events = user.UncommittedDomainEvents;

        _userRepository.Save(user);

        _eventDispatcher.Dispatch(events);
    }
}
