using FluentValidation;
using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class LinkRunescapeAccountHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher,
    IValidator<LinkRunescapeAccountRequest> validator) : ICommandHandler<LinkRunescapeAccountRequest>
{
    public async Task<CommandResult> Handle(LinkRunescapeAccountRequest request, CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(request);

        if (!validation.IsValid)
            return CommandResult.Fail(string.Join(
                "; ",
                validation.Errors.Select(e => e.ErrorMessage)));

        return await CommandHandler.ExecuteAsync(async () =>
        {
            var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

            user.AddRunescapeAccount(request.ActingUserId, request.Username);

            var events = await userRepository.SaveAsync(user, cancellationToken);

            eventDispatcher.Dispatch(events);
        });
    }
}