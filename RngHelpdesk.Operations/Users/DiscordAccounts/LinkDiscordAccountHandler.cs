using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Discord;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users.DiscordAccounts;

public sealed class LinkDiscordAccountHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IDiscordUsernameResolver _discordResolver;

    public LinkDiscordAccountHandler(
        IUserRepository userRepository,
        IEventDispatcher eventDispatcher,
        IDiscordUsernameResolver discordResolver)
    {
        _userRepository = userRepository;
        _eventDispatcher = eventDispatcher;
        _discordResolver = discordResolver;
    }

    public async Task<CommandResult> HandleAsync(
        IRequestContext requestContext,
        LinkDiscordAccountRequest request,
        CancellationToken ct = default)
    {
        AuthorizationRules.RequireAdminRole(requestContext);

        var username = request.Username;
        if (string.IsNullOrWhiteSpace(username))
        {
            username = await _discordResolver.ResolveUsernameAsync(request.DiscordId, ct);
            if (string.IsNullOrWhiteSpace(username))
                return CommandResult.Fail("Could not resolve Discord username. Ensure the DiscordBot is running and the user is in a shared server.");
        }

        var user = _userRepository.GetById(request.UserId);

        user.AddDiscordAccount(
            request.DiscordId,
            username);

        var events = _userRepository.Save(user);

        _eventDispatcher.Dispatch(events);

        return CommandResult.Ok();
    }
}
