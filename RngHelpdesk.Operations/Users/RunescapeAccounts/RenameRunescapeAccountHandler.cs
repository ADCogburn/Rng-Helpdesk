using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class RenameRunescapeAccountHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher)
{
    public CommandResult Handle(RenameRunescapeAccountRequest request)
    {
        return CommandHandler.Execute(() =>
        {
            var user = userRepository.GetById(request.UserId);

            user.RenameRunescapeAccount(
                request.ActingUserId,
                request.OldUsername,
                request.NewUsername);

            var events = userRepository.Save(user);

            eventDispatcher.Dispatch(events);
        });
    }
}