using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;

public sealed class CreateUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ICredentialStore _credentialStore;

    public CreateUserHandler(
        IUserRepository userRepository,
        IEventDispatcher eventDispatcher,
        ICredentialStore credentialStore)
    {
        _userRepository = userRepository;
        _eventDispatcher = eventDispatcher;
        _credentialStore = credentialStore;
    }

    public CommandResult<CreateUserResponse> Handle(
        ulong actingUserId,
        CreateUserRequest request)
    {
        if (request.DiscordAccount == null)
            return CommandResult<CreateUserResponse>.Fail("Discord account is required.");

        if (request.DiscordAccount.DiscordId == 0 || string.IsNullOrWhiteSpace(request.DiscordAccount.Username))
            return CommandResult<CreateUserResponse>.Fail("Discord account, its snowflake Id, and its username are required.");

        if (_userRepository.UserExistsWithDiscordId(request.DiscordAccount.DiscordId) || _userRepository.UserExistsWithDiscordUsername(request.DiscordAccount.Username))
            return CommandResult<CreateUserResponse>.Fail("User with this Discord ID or username already exists.");

        var discordAccount = new DiscordAccount(request.DiscordAccount.DiscordId, request.DiscordAccount.Username);

        var runescapeAccounts = new List<RunescapeAccount>();

        if (request.RunescapeAccounts != null && request.RunescapeAccounts.Count > 0)
        {
            runescapeAccounts = request.RunescapeAccounts
                .Select(x => new RunescapeAccount(x.Username))
                .ToList();
        }

        var user = User.Create(
           actingUserId,
           discordAccount,
           runescapeAccounts);

        var events = _userRepository.Save(user);

        _eventDispatcher.Dispatch(events);

        // TODO: Rigorous name setup

        var preferredUsername = runescapeAccounts.Count > 0
            ? runescapeAccounts.First().Username
            : discordAccount.Username;

        var (username, password) =
            _credentialStore.CreateTemporaryCredentials(
                user.Id,
                preferredUsername);

        return CommandResult<CreateUserResponse>.Ok(new CreateUserResponse
        {
            UserId = user.Id,
            Username = username,
            TemporaryPassword = password
        });
    }
}