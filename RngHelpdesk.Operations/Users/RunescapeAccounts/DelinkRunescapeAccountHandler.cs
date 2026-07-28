using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class DelinkRunescapeAccountHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher) : ICommandHandler<DelinkRunescapeAccountRequest>
{
    public async Task<CommandResult> Handle(DelinkRunescapeAccountRequest request, CancellationToken cancellationToken = default)
    {
        return await CommandHandler.ExecuteAsync(async () =>
        {
            var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

            user.RemoveRunescapeAccount(request.ActingUserId, request.Username);

            var events = await userRepository.SaveAsync(user, cancellationToken);

            eventDispatcher.Dispatch(events);
        });
    }
}