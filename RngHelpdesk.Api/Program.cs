using FluentValidation;
using FluentValidation.AspNetCore;
using RngHelpdesk.Api.Validators.Users;
using RngHelpdesk.Operations.Handlers.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LinkRunescapeAccountRequestValidator>();

// DI Container

builder.Services.AddScoped<GetUserHandler>();
builder.Services.AddScoped<GetRunescapeAccountHandler>();
builder.Services.AddScoped<LinkRunescapeAccountHandler>();
builder.Services.AddScoped<LinkDiscordAccountHandler>();


var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
