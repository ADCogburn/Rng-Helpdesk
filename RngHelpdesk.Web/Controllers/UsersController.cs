using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Web.Models.Users;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RngHelpdesk.Web.Controllers;

[Authorize]
public sealed class UsersController(IHttpClientFactory httpClientFactory) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("JwtToken");

        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Login", "Auth");

        var client = httpClientFactory.CreateClient("RngHelpdeskApi");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/users");

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!response.IsSuccessStatusCode)
            return View(new GetAllUsersResponseViewModel());

        var result = await response.Content
            .ReadFromJsonAsync<GetAllUsersResponseViewModel>(JsonOptions);

        return View(result ?? new GetAllUsersResponseViewModel());
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
}