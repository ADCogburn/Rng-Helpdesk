using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Points;

public sealed class RemovePointsFromUserHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher)
{
    public CommandResult Handle(RemovePointsFromUserRequest request)
    {
        return CommandHandler.Execute(() =>
        {
            var user = userRepository.GetById(request.UserId);

            user.DeductClanPoints(
                request.ActingUserId,
                request.Points,
                request.Reason);

            var events = userRepository.Save(user);

            eventDispatcher.Dispatch(events);
        });
    }
}
