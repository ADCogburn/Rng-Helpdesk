using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users.DiscordAccounts;

public sealed class LinkDiscordAccountHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEventDispatcher _eventDispatcher;

    public LinkDiscordAccountHandler(
        IUserRepository userRepository,
        IEventDispatcher eventDispatcher)
    {
        _userRepository = userRepository;
        _eventDispatcher = eventDispatcher;
    }

    public CommandResult Handle(
        IRequestContext requestContext,
        LinkDiscordAccountRequest request)
    {
        AuthorizationRules.RequireAdminRole(requestContext);

        var user = _userRepository.GetById(request.UserId);

        user.AddDiscordAccount(
            request.DiscordId,
            request.Username); // supplied by adapter

        var events = _userRepository.Save(user);

        _eventDispatcher.Dispatch(events);

        return CommandResult.Ok();
    }
}
