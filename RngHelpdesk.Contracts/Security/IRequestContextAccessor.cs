namespace RngHelpdesk.Contracts.Security;

public interface IRequestContextAccessor
{
    IRequestContext Context { get; }
}
