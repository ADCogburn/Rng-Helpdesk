using Microsoft.AspNetCore.Authentication.Cookies;
using RngHelpdesk.Contracts.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });


builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy(AuthPolicies.AdminPlus, policy =>
        policy.RequireRole(
            AppRole.Administrator.ToString(),
            AppRole.SuperAdministrator.ToString(),
            AppRole.Owner.ToString()));

    opt.AddPolicy(AuthPolicies.OwnerOnly, policy =>
        policy.RequireRole(
            AppRole.Owner.ToString()));

    opt.AddPolicy(AuthPolicies.DiscordBotOnly, policy =>
        policy.RequireClaim("client_type", "discord_bot"));
});

builder.Services.AddSession();

builder.Services.AddHttpClient("RngHelpdeskApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();