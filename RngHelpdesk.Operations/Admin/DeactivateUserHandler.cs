namespace RngHelpdesk.Operations.Admin;

public sealed class DeactivateUserHandler
{
    //private readonly IUserRepository _userRepository;
    //private readonly IEventDispatcher _eventDispatcher;

    //public DeactivateUserHandler(
    //    IUserRepository userRepository,
    //    IEventDispatcher eventDispatcher)
    //{
    //    _userRepository = userRepository;
    //    _eventDispatcher = eventDispatcher;
    //}

    //public CommandResult Handle(
    //    IRequestContext context,
    //    DeactivateUserRequest request)
    //{
    //    AuthorizationRules.RequireAdminRole(context);

    //    var user = _userRepository.GetById(request.UserId);

    //    user.Deactivate();

    //    var events = _userRepository.Save(user);
    //    _eventDispatcher.Dispatch(events);

    //    return CommandResult.Ok();
    //}
}