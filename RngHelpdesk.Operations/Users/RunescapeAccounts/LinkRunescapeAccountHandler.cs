using FluentValidation;
using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class LinkRunescapeAccountHandler(
    IUserRepository userRepository,
    IEventDispatcher eventDispatcher,
    IValidator<LinkRunescapeAccountRequest> validator)
{
    public CommandResult Handle(LinkRunescapeAccountRequest request)
    {
        var validation = validator.Validate(request);

        if (!validation.IsValid)
            return CommandResult.Fail(string.Join(
                "; ",
                validation.Errors.Select(e => e.ErrorMessage)));

        return CommandHandler.Execute(() =>
        {
            var user = userRepository.GetById(request.UserId);

            user.AddRunescapeAccount(request.ActingUserId, request.Username);

            var events = userRepository.Save(user);

            eventDispatcher.Dispatch(events);
        });
    }
}