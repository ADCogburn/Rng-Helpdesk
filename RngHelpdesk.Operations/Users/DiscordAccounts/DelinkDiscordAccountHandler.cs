using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users.DiscordAccounts;

public sealed class DelinkDiscordAccountHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEventDispatcher _eventDispatcher;

    public DelinkDiscordAccountHandler(
        IUserRepository userRepository,
        IEventDispatcher eventDispatcher)
    {
        _userRepository = userRepository;
        _eventDispatcher = eventDispatcher;
    }

    public CommandResult Handle(
        IRequestContext requestContext,
        DelinkDiscordAccountRequest request)
    {
        AuthorizationRules.RequireAdminRole(requestContext);

        var user = _userRepository.GetById(request.UserId);

        user.RemoveDiscordAccount(request.DiscordId);

        var events = _userRepository.Save(user);

        _eventDispatcher.Dispatch(events);

        return CommandResult.Ok();
    }
}