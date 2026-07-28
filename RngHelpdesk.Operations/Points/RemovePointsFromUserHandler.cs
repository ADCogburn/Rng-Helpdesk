using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Points;

public sealed class RemovePointsFromUserHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher) : ICommandHandler<RemovePointsFromUserRequest>
{
    public Task<CommandResult> Handle(RemovePointsFromUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = CommandHandler.Execute(() =>
        {
            var user = userRepository.GetById(request.UserId);

            user.DeductClanPoints(
                request.ActingUserId,
                request.Points,
                request.Reason);

            var events = userRepository.Save(user);

            eventDispatcher.Dispatch(events);
        });

        return Task.FromResult(result);
    }
}
