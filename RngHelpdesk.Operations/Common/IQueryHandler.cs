using RngHelpdesk.Contracts.Common;

namespace RngHelpdesk.Operations.Common;

public interface IQueryHandler<TRequest, TResponse>
{
    Task<QueryResult<TResponse>> Handle(TRequest request, CancellationToken cancellationToken = default);
}
