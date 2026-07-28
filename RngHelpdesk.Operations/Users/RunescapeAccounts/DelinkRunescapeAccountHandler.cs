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
    public Task<CommandResult> Handle(DelinkRunescapeAccountRequest request, CancellationToken cancellationToken = default)
    {
        var result = CommandHandler.Execute(() =>
        {
            var user = userRepository.GetById(request.UserId);

            user.RemoveRunescapeAccount(request.ActingUserId, request.Username);

            var events = userRepository.Save(user);

            eventDispatcher.Dispatch(events);
        });

        return Task.FromResult(result);
    }
}