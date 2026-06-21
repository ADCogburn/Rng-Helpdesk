using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Web.Models.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RngHelpdesk.Web.Controllers;

public sealed class AuthController(IHttpClientFactory httpClientFactory) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        var token = HttpContext.Session.GetString("JwtToken");

        if (!string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var client = httpClientFactory.CreateClient("RngHelpdeskApi");

        var response = await client.PostAsJsonAsync("/auth/login", model);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (loginResponse is null || string.IsNullOrWhiteSpace(loginResponse.Token))
        {
            ModelState.AddModelError("", "Login failed.");
            return View(model);
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(loginResponse.Token);

        var claims = jwt.Claims.ToList();

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        HttpContext.Session.SetString("JwtToken", loginResponse.Token);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        HttpContext.Session.Remove("JwtToken");

        return RedirectToAction("Index", "Home");
    }
}