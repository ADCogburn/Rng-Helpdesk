using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RngHelpdesk.Api.Tests.Controllers;

internal static class ControllerTestHelpers
{
    /// <summary>
    /// Stamps a ClaimsPrincipal with a NameIdentifier claim onto the controller's HttpContext,
    /// so User.GetUserId() (Api/Helpers/ClaimsPrincipalExtensions.cs) resolves without a real
    /// auth pipeline.
    /// </summary>
    public static void SetActingUser(ControllerBase controller, ulong userId)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            authenticationType: "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }
}
