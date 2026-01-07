using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

public sealed class CreateUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEventDispatcher _eventDispatcher;

    public CreateUserHandler(
        IUserRepository userRepository,
        IEventDispatcher eventDispatcher)
    {
        _userRepository = userRepository;
        _eventDispatcher = eventDispatcher;
    }

    public CommandResult Handle(
        IRequestContext context,
        CreateUserRequest request)
    {
        AuthorizationRules.RequireSuperAdminRole(context);

        if (_userRepository.Exists(request.UserId))
            return CommandResult.Fail("User already exists.");

        var discordAccounts = request.DiscordAccounts.Select(x => new DiscordAccount(x.DiscordId, x.Username));

        var runescapeAccounts = request.RunescapeAccounts.Select(x => new RunescapeAccount(x.Username));

        var user = User.Create(
            request.UserId,
            request.AuthorityRole,
            discordAccounts,
            runescapeAccounts);

        var events = _userRepository.Save(user);

        _eventDispatcher.Dispatch(events);

        return CommandResult.Ok();
    }
}
