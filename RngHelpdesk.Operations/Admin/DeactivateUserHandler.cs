using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Admin;

public sealed class DeactivateUserHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher) : ICommandHandler<DeactivateUserRequest>
{
    public async Task<CommandResult> Handle(DeactivateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await CommandHandler.ExecuteAsync(async () =>
        {
            var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

            user.Deactivate(request.ActingUserId);

            var events = await userRepository.SaveAsync(user, cancellationToken);

            eventDispatcher.Dispatch(events);
        });
    }
}
