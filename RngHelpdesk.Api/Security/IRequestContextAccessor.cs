using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Api.Security;

public interface IRequestContextAccessor
{
    IRequestContext Context { get; }
}