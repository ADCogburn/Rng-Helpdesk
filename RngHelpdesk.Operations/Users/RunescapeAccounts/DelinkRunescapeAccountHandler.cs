using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class DelinkRunescapeAccountHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher)
{
    public CommandResult Handle(DelinkRunescapeAccountRequest request)
    {
        return CommandHandler.Execute(() =>
        {
            var user = userRepository.GetById(request.UserId);

            user.RemoveRunescapeAccount(request.ActingUserId, request.Username);

            var events = userRepository.Save(user);

            eventDispatcher.Dispatch(events);
        });
    }
}