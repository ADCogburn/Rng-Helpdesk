using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Api.Validators.Users;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using RngHelpdesk.Infrastructure.Persistence.Points;
using RngHelpdesk.Infrastructure.Persistence.Projections;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Admin;
using RngHelpdesk.Operations.Points;
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
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LinkRunescapeAccountRequestValidator>();

// --- *** --- DI Container --- *** ---

// -- Adapter Level Services --

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IRequestContextAccessor, HttpRequestContextAccessor>();
builder.Services.AddScoped<AuthorizationService>();

// -- Operations Level Handlers --

builder.Services.AddScoped<ChangeUserRoleHandler>();

builder.Services.AddSingleton<IActorUserResolver, PostgresActorUserResolver>();

builder.Services.AddScoped<IEventStore, PostgresEventStore>();
builder.Services.AddScoped<IEventStoreMetadataProvider, RequestContextEventStoreMetadataProvider>();

builder.Services.AddScoped<PostgresRankThresholdProvider>();
builder.Services.AddSingleton<IRankThresholdProvider, CachingRankThresholdProvider>();
builder.Services.AddSingleton<RankResolver>();

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

builder.Services.AddScoped<IUserRepository, InMemUserRepository>();
builder.Services.AddScoped<IAuthStore, InMemoryAuthStore>();

builder.Services.AddHttpClient<RngHelpdesk.Infrastructure.Discord.HttpDiscordUsernameResolver>(client =>
{
    var baseUrl = builder.Configuration["DiscordBot:BaseUrl"] ?? "http://localhost:59854";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddScoped<RngHelpdesk.Contracts.Discord.IDiscordUsernameResolver>(
    sp => sp.GetRequiredService<RngHelpdesk.Infrastructure.Discord.HttpDiscordUsernameResolver>());

// -- Projection (Singleton so read models are shared; InMemEventDispatcher captures them) --

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

builder.Services.AddScoped<IProjectionCheckpointStore, PostgresProjectionCheckpointStore>();

builder.Services.AddScoped<ProjectionRunner>(sp =>
{
    return new ProjectionRunner(
        sp.GetRequiredService<IEventStore>(),
        sp.GetRequiredService<EventTypeRegistry>(),
        sp.GetRequiredService<IProjectionCheckpointStore>(),
        new object[]
        {
            sp.GetRequiredService<PointHistoryProjection>(),
            sp.GetRequiredService<UserSummaryProjection>(),
            sp.GetRequiredService<UserLifecycleHistoryProjection>(),
            sp.GetRequiredService<UserPointsTotalProjection>(),
            sp.GetRequiredService<RunescapeAccountHistoryProjection>(),
            sp.GetRequiredService<DiscordAccountHistoryProjection>()
        });
});

// Register the mapped Domain Events -> String names for permanent linkage, even if the classes change over time.
var registry = EventStoreRegistration.CreateRegistry();
builder.Services.AddSingleton(registry);

builder.Services.AddSingleton(NpgsqlDataSource.Create(builder.Configuration.GetConnectionString("RngHelpdeskDB")));

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("RngHelpdeskDB")));

var app = builder.Build();

// TEMP: Seed in-mem data for debugging
//var userRepo = app.Services
//    .GetRequiredService<IUserRepository>() as InMemUserRepository;

//var dispatcher = app.Services
//    .GetRequiredService<IEventDispatcher>();

//var seedEvents = new IDomainEvent[]
//{
//    new UserCreatedEvent(
//        userId: 1,
//        authorityRole: AuthorityRole.Administrator,
//        discordAccounts: new[]
//        {
//            new DiscordAccount(
//                123456789012345678,
//                "Seeded Discord Account")
//        },
//        runescapeAccounts: Array.Empty<RunescapeAccount>()
//    )
//};

//userRepo!.Seed(1, seedEvents);

//dispatcher.Dispatch(seedEvents);

//var actorResolver = app.Services
//    .GetRequiredService<IActorUserResolver>();

//actorResolver.RegisterActor(
//    actorId: Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
//    actorType: ActorType.WebUser,
//    userId: 1
//);

//var authStore = app.Services.GetRequiredService<InMemoryAuthStore>();

//authStore.SeedUser(
//    userId: 1,
//    actorId: Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
//    username: "admin",
//    password: "password",
//    mustChangePassword: false
//);

// --- END TEMP ---

using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<ProjectionRunner>();
    await runner.RunAsync();
}

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