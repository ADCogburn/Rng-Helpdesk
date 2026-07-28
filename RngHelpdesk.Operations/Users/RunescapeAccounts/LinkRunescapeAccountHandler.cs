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
    public Task<CommandResult> Handle(LinkRunescapeAccountRequest request, CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(request);

        if (!validation.IsValid)
            return Task.FromResult(CommandResult.Fail(string.Join(
                "; ",
                validation.Errors.Select(e => e.ErrorMessage))));

        var result = CommandHandler.Execute(() =>
        {
            var user = userRepository.GetById(request.UserId);

            user.AddRunescapeAccount(request.ActingUserId, request.Username);

            var events = userRepository.Save(user);

            eventDispatcher.Dispatch(events);
        });

        return Task.FromResult(result);
    }
}