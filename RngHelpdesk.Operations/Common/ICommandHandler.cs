using RngHelpdesk.Contracts.Common;

namespace RngHelpdesk.Operations.Common;

public interface ICommandHandler<TRequest>
{
    Task<CommandResult> Handle(TRequest request, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<TRequest, TResult>
{
    Task<CommandResult<TResult>> Handle(TRequest request, CancellationToken cancellationToken = default);
}
