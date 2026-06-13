using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Points;

public sealed class AddPointsToUserHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher)
{
    public CommandResult Handle(AddPointsToUserRequest request)
    {
        return CommandHandler.Execute(() =>
        {
            var user = userRepository.GetById(request.UserId);

            user.AddClanPoints(
                request.ActingUserId,
                request.Points,
                request.Reason);

            var events = userRepository.Save(user);

            eventDispatcher.Dispatch(events);
        });
    }
}
