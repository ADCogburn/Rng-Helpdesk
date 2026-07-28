using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Points;

public sealed class AddPointsToUserHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher) : ICommandHandler<AddPointsToUserRequest>
{
    public async Task<CommandResult> Handle(AddPointsToUserRequest request, CancellationToken cancellationToken = default)
    {
        return await CommandHandler.ExecuteAsync(async () =>
        {
            var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

            user.AddClanPoints(
                request.ActingUserId,
                request.Points,
                request.Reason);

            var events = await userRepository.SaveAsync(user, cancellationToken);

            eventDispatcher.Dispatch(events);
        });
    }
}
