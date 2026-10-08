using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

public sealed class CreateUserHandler(
    IUserLookupReadStore userLookupReadStore,
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher,
    ICredentialStore credentialStore) : ICommandHandler<CreateUserRequest, CreateUserResponse>
{
    public async Task<CommandResult<CreateUserResponse>> Handle(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DiscordAccount == null)
            return CommandResult<CreateUserResponse>.Fail("Discord account is required.");

        if (request.DiscordAccount.DiscordId == 0 || string.IsNullOrWhiteSpace(request.DiscordAccount.Username))
            return CommandResult<CreateUserResponse>.Fail("Discord account, its snowflake Id, and its username are required.");

        if (await userLookupReadStore.ExistsWithDiscordIdAsync(request.DiscordAccount.DiscordId, cancellationToken)
            || await userLookupReadStore.ExistsWithDiscordUsernameAsync(request.DiscordAccount.Username, cancellationToken))
            return CommandResult<CreateUserResponse>.Fail("User with this Discord ID or username already exists.");

        return await CommandHandler.ExecuteAsync(async () =>
        {
            var discordAccount = new DiscordAccount(
                request.DiscordAccount.DiscordId,
                request.DiscordAccount.Username);

            var runescapeAccounts = request.RunescapeAccounts is { Count: > 0 }
                ? request.RunescapeAccounts
                    .Select(x => new RunescapeAccount(x.Username))
                    .ToList()
                : new List<RunescapeAccount>();

            var user = User.Create(
                request.ActingUserId,
                discordAccount,
                runescapeAccounts);

            var events = await userRepository.SaveAsync(user, cancellationToken);

            eventDispatcher.Dispatch(events);

            var preferredUsername = runescapeAccounts.Count > 0
                ? runescapeAccounts.First().Username
                : discordAccount.Username;

            var (username, password) =
                await credentialStore.CreateTemporaryCredentialsAsync(
                    user.Id,
                    preferredUsername,
                    cancellationToken);

            return new CreateUserResponse
            {
                UserId = user.Id,
                Username = username,
                TemporaryPassword = password
            };
        });
    }
}