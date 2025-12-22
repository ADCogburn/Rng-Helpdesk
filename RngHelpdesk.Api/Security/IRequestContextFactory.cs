using RngHelpdesk.Contracts.Security;

public interface IRequestContextFactory
{
    IRequestContext CreateHttpContext(HttpContext httpContext);
}