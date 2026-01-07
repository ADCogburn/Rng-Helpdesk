using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Admin;

public sealed class ReactivateUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEventDispatcher _eventDispatcher;

    public ReactivateUserHandler(
        IUserRepository userRepository,
        IEventDispatcher eventDispatcher)
    {
        _userRepository = userRepository;
        _eventDispatcher = eventDispatcher;
    }

    public CommandResult Handle(
        IRequestContext context,
        ReactivateUserRequest request)
    {
        AuthorizationRules.RequireAdminRole(context);

        var user = _userRepository.GetById(request.UserId);

        user.Reactivate();

        var events = _userRepository.Save(user);
        _eventDispatcher.Dispatch(events);

        return CommandResult.Ok();
    }
}