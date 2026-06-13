using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;

public sealed class CreateUserHandler(
    IUserLookupReadStore userLookupReadStore,
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher,
    ICredentialStore credentialStore)
{
    public CommandResult<CreateUserResponse> Handle(
        ulong actingUserId,
        CreateUserRequest request)
    {
        if (request.DiscordAccount == null)
            return CommandResult<CreateUserResponse>.Fail("Discord account is required.");

        if (request.DiscordAccount.DiscordId == 0 || string.IsNullOrWhiteSpace(request.DiscordAccount.Username))
            return CommandResult<CreateUserResponse>.Fail("Discord account, its snowflake Id, and its username are required.");

        if (userLookupReadStore.ExistsWithDiscordId(request.DiscordAccount.DiscordId)
            || userLookupReadStore.ExistsWithDiscordUsername(request.DiscordAccount.Username))
            return CommandResult<CreateUserResponse>.Fail("User with this Discord ID or username already exists.");


        return CommandHandler.Execute(() =>
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
                actingUserId,
                discordAccount,
                runescapeAccounts);

            var events = userRepository.Save(user);

            eventDispatcher.Dispatch(events);

            var preferredUsername = runescapeAccounts.Count > 0
                ? runescapeAccounts.First().Username
                : discordAccount.Username;

            var (username, password) =
                credentialStore.CreateTemporaryCredentials(
                    user.Id,
                    preferredUsername);

            return new CreateUserResponse
            {
                UserId = user.Id,
                Username = username,
                TemporaryPassword = password
            };
        });
    }
}