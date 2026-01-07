using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Security;

public sealed class CreateUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly InMemoryAuthStore _authStore;
    private readonly IActorUserResolver _actorResolver;

    public CreateUserHandler(
        IUserRepository userRepository,
        IEventDispatcher eventDispatcher,
        InMemoryAuthStore authStore,
        IActorUserResolver actorResolver)
    {
        _userRepository = userRepository;
        _eventDispatcher = eventDispatcher;
        _authStore = authStore;
        _actorResolver = actorResolver;
    }

    public CommandResult<CreateUserResponse> Handle(
        IRequestContext context,
        CreateUserRequest request)
    {
        AuthorizationRules.RequireAdminRole(context);

        if (_userRepository.Exists(request.UserId))
            return CommandResult<CreateUserResponse>.Fail("User already exists.");

        var discordAccounts = request.DiscordAccounts
            .Select(x => new DiscordAccount(x.DiscordId, x.Username));

        var runescapeAccounts = request.RunescapeAccounts
            .Select(x => new RunescapeAccount(x.Username))
            .ToList();

        var user = User.Create(
            request.UserId,
            request.AuthorityRole,
            discordAccounts,
            runescapeAccounts);

        var events = _userRepository.Save(user);
        _eventDispatcher.Dispatch(events);

        var actorId = Guid.NewGuid();

        _actorResolver.RegisterActor(
            actorId,
            ActorType.WebUser,
            request.UserId);

        var preferredUsername = runescapeAccounts.First().Username;

        var (username, password) =
            _authStore.CreateTemporaryCredentials(
                request.UserId,
                actorId,
                preferredUsername);

        return CommandResult<CreateUserResponse>.Ok(new CreateUserResponse
        {
            UserId = request.UserId,
            Username = username,
            TemporaryPassword = password
        });
    }
}