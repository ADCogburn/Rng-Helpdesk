using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Admin;

public sealed class ReactivateUserHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher) : ICommandHandler<ReactivateUserRequest>
{
    public async Task<CommandResult> Handle(ReactivateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await CommandHandler.ExecuteAsync(async () =>
        {
            var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

            user.Reactivate(request.ActingUserId);

            var events = await userRepository.SaveAsync(user, cancellationToken);

            eventDispatcher.Dispatch(events);
        });
    }
}
