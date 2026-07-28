using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class RenameRunescapeAccountHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher) : ICommandHandler<RenameRunescapeAccountRequest>
{
    public async Task<CommandResult> Handle(RenameRunescapeAccountRequest request, CancellationToken cancellationToken = default)
    {
        return await CommandHandler.ExecuteAsync(async () =>
        {
            var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

            user.RenameRunescapeAccount(
                request.ActingUserId,
                request.OldUsername,
                request.NewUsername);

            var events = await userRepository.SaveAsync(user, cancellationToken);

            eventDispatcher.Dispatch(events);
        });
    }
}