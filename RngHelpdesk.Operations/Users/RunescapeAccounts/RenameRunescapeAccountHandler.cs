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
    public Task<CommandResult> Handle(RenameRunescapeAccountRequest request, CancellationToken cancellationToken = default)
    {
        var result = CommandHandler.Execute(() =>
        {
            var user = userRepository.GetById(request.UserId);

            user.RenameRunescapeAccount(
                request.ActingUserId,
                request.OldUsername,
                request.NewUsername);

            var events = userRepository.Save(user);

            eventDispatcher.Dispatch(events);
        });

        return Task.FromResult(result);
    }
}