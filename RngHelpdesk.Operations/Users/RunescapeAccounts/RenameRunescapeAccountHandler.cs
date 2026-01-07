using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class RenameRunescapeAccountHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEventDispatcher _eventDispatcher;

    public RenameRunescapeAccountHandler(
        IUserRepository userRepository,
        IEventDispatcher eventDispatcher)
    {
        _userRepository = userRepository;
        _eventDispatcher = eventDispatcher;
    }

    public CommandResult Handle(
        IRequestContext requestContext,
        RenameRunescapeAccountRequest request)
    {
        AuthorizationRules.RequireAdminRole(requestContext);

        var user = _userRepository.GetById(request.UserId);

        user.RenameRunescapeAccount(
            request.OldUsername,
            request.NewUsername);

        var events = _userRepository.Save(user);

        _eventDispatcher.Dispatch(events);

        return CommandResult.Ok();
    }
}