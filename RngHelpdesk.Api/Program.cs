using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Api.Validators.Users;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Admin;
using RngHelpdesk.Operations.Points;
using RngHelpdesk.Operations.Ranks;
using RngHelpdesk.Operations.Security;
using RngHelpdesk.Operations.Users;
using RngHelpdesk.Operations.Users.DiscordAccounts;
using RngHelpdesk.Operations.Users.RunescapeAccounts;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement((document) => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // TODO: change to true in production
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy
            .WithOrigins("http://localhost:55751")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LinkRunescapeAccountRequestValidator>();

// --- *** --- DI Container --- *** ---

// -- Adapter Level Services --

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IRequestContextAccessor, HttpRequestContextAccessor>();
builder.Services.AddScoped<IRequestContextFactory, HttpRequestContextFactory>();
builder.Services.AddScoped<AuthorizationService>();

// -- Operations Level Handlers --

builder.Services.AddScoped<ChangeAdminStatusHandler>();

builder.Services.AddSingleton<IActorUserResolver, InMemoryActorUserResolver>();

builder.Services.AddSingleton<RankResolver>();
builder.Services.AddSingleton<IEnumerable<RankThreshold>>(new[]
{
    new RankThreshold(Rank.Bronze, 0),
    new RankThreshold(Rank.Iron, 100),
    new RankThreshold(Rank.Steel, 200),
    new RankThreshold(Rank.Mithril, 300),
    new RankThreshold(Rank.Adamant, 400),
    new RankThreshold(Rank.Rune, 500),
    new RankThreshold(Rank.Dragon, 600),
    new RankThreshold(Rank.Sapphire, 700),
    new RankThreshold(Rank.Emerald, 800),
    new RankThreshold(Rank.Ruby, 900),
    new RankThreshold(Rank.Diamond, 1000),
    new RankThreshold(Rank.Dragonstone, 1100),
    new RankThreshold(Rank.Onyx, 1200),
    new RankThreshold(Rank.Zenyte, 1300)
});

builder.Services.AddScoped<GetAllUsersHandler>();
builder.Services.AddScoped<GetUserHandler>();
builder.Services.AddScoped<GetUserLifecycleHistoryHandler>();
builder.Services.AddScoped<CreateUserHandler>();

builder.Services.AddScoped<GetRunescapeAccountHandler>();
builder.Services.AddScoped<GetRunescapeAccountHistoryHandler>();
builder.Services.AddScoped<LinkRunescapeAccountHandler>();
builder.Services.AddScoped<DelinkRunescapeAccountHandler>();
builder.Services.AddScoped<RenameRunescapeAccountHandler>();

builder.Services.AddScoped<LinkDiscordAccountHandler>();
builder.Services.AddScoped<DelinkDiscordAccountHandler>();

builder.Services.AddScoped<AddPointsToUserHandler>();
builder.Services.AddScoped<RemovePointsFromUserHandler>();
builder.Services.AddScoped<GetPointHistoryForUserHandler>();

// -- Repositories --

builder.Services.AddSingleton<IUserRepository, InMemUserRepository>();
builder.Services.AddSingleton<InMemoryAuthStore>();

// -- Projection --

builder.Services.AddSingleton<PointHistoryProjection>();
builder.Services.AddSingleton<UserSummaryProjection>();
builder.Services.AddSingleton<UserLifecycleHistoryProjection>();
builder.Services.AddSingleton<UserPointsTotalProjection>();
builder.Services.AddSingleton<RunescapeAccountHistoryProjection>();
builder.Services.AddSingleton<DiscordAccountHistoryProjection>();

// -- Event Dispatcher --

builder.Services.AddSingleton<IEventDispatcher>(sp =>
{
    var handlers = new object[]
    {
        sp.GetRequiredService<PointHistoryProjection>(),
        sp.GetRequiredService<UserSummaryProjection>(),
        sp.GetRequiredService<UserLifecycleHistoryProjection>(),
        sp.GetRequiredService<UserPointsTotalProjection>(),
        sp.GetRequiredService<RunescapeAccountHistoryProjection>(),
        sp.GetRequiredService<DiscordAccountHistoryProjection>()
    };

    return new InMemEventDispatcher(handlers);
});

var app = builder.Build();

// TEMP: Seed in-mem data for debugging
var userRepo = app.Services
    .GetRequiredService<IUserRepository>() as InMemUserRepository;

var dispatcher = app.Services
    .GetRequiredService<IEventDispatcher>();

var seedEvents = new IDomainEvent[]
{
    new UserCreatedEvent(
        userId: 1,
        authorityRole: AuthorityRole.Administrator,
        discordAccounts: new[]
        {
            new DiscordAccount(
                123456789012345678,
                "Seeded Discord Account")
        },
        runescapeAccounts: Array.Empty<RunescapeAccount>()
    )
};

userRepo!.Seed(1, seedEvents);

dispatcher.Dispatch(seedEvents);

var actorResolver = app.Services
    .GetRequiredService<IActorUserResolver>();

actorResolver.RegisterActor(
    actorId: Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
    actorType: ActorType.WebUser,
    userId: 1
);

var authStore = app.Services.GetRequiredService<InMemoryAuthStore>();

authStore.SeedUser(
    userId: 1,
    actorId: Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
    username: "admin",
    password: "password",
    mustChangePassword: false
);

// --- END TEMP ---

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("DevCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();