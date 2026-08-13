using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Api.Validators.Users;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Common.Ranks.Commands;
using RngHelpdesk.Contracts.Common.Ranks.Queries;
using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Contracts.Points.Queries;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using RngHelpdesk.Infrastructure.Persistence.Points;
using RngHelpdesk.Infrastructure.Persistence.Projections;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Infrastructure.Users.RunescapeAccount;
using RngHelpdesk.Operations.Admin;
using RngHelpdesk.Operations.Common;
using RngHelpdesk.Operations.Points;
using RngHelpdesk.Operations.Services;
using RngHelpdesk.Operations.Users;
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
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RngHelpdesk API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter ONLY your JWT token: {your JWT token}"
    });

    options.AddSecurityRequirement((document) => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // dev only
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
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

    opt.AddPolicy(AuthPolicies.DiscordBotOnly, policy =>
        policy.RequireClaim("client_type", "discord_bot"));
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LinkRunescapeAccountRequestValidator>();

// --- *** --- DI Container --- *** --- 

// -- Operations Level Handlers --

builder.Services.AddScoped<ICommandHandler<ChangeUserRoleCommand>, ChangeUserRoleHandler>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();

var connectionString = builder.Configuration.GetConnectionString("RngHelpdeskDB")
    ?? throw new InvalidOperationException("Missing required connection string 'RngHelpdeskDB'.");

void ConfigureAppDbContext(DbContextOptionsBuilder options) => options.UseNpgsql(connectionString);

builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddDbContext<AppDbContext>(ConfigureAppDbContext);

builder.Services.AddSingleton<IEventStore, PostgresEventStore>();

// Scoped, matching AppDbContext's own (scoped) lifetime -- no captive-dependency mismatch to
// bridge with a caching wrapper the way the old commented-out CachingRankThresholdProvider did.
builder.Services.AddScoped<IRankThresholdProvider, PostgresRankThresholdProvider>();

// Write side for rank thresholds (issue #17), kept separate from the read-only provider above --
// same scoped/AppDbContext lifetime. Note this does NOT refresh the RankResolver singleton below,
// which is built once from a startup snapshot; an admin edit here won't affect rank resolution
// until the next restart (see CLAUDE.md's Runtime reality section).
builder.Services.AddScoped<IRankThresholdRepository, PostgresRankThresholdRepository>();

// RankResolver is a singleton built once at startup from a point-in-time snapshot of
// thresholds (not re-resolved per request), so it needs its own short-lived AppDbContext here,
// ahead of the DI container existing to hand out scoped ones.
var thresholdOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
ConfigureAppDbContext(thresholdOptionsBuilder);
var thresholdDbContextOptions = thresholdOptionsBuilder.Options;

IReadOnlyList<RankThreshold> rankThresholds;
await using (var thresholdDbContext = new AppDbContext(thresholdDbContextOptions))
{
    rankThresholds = await new PostgresRankThresholdProvider(thresholdDbContext).GetThresholdsAsync();
}

builder.Services.AddSingleton(new RankResolver(rankThresholds));

builder.Services.AddScoped<IQueryHandler<GetAllUsersQuery, GetAllUsersResponse>, GetAllUsersHandler>();
builder.Services.AddScoped<IQueryHandler<GetUserByIdQuery, GetUserResponse>, GetUserByIdHandler>();
builder.Services.AddScoped<IQueryHandler<GetUserByRunescapeUsernameQuery, GetUserResponse>, GetUserByRunescapeUsernameHandler>();
builder.Services.AddScoped<IQueryHandler<GetUsersByHistoricalRunescapeUsernameQuery, GetUsersResponse>, GetUsersHandler>();
builder.Services.AddScoped<IQueryHandler<GetUserLifecycleHistoryQuery, GetUserLifecycleHistoryResponse>, GetUserLifecycleHistoryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateUserRequest, CreateUserResponse>, CreateUserHandler>();

builder.Services.AddScoped<IQueryHandler<GetRunescapeAccountsQuery, GetRunescapeAccountsResponse>, GetRunescapeAccountHandler>();
builder.Services.AddScoped<IQueryHandler<GetPreviousRunescapeAccountsQuery, GetRunescapeAccountsResponse>, GetPreviousRunescapeAccountsHandler>();
builder.Services.AddScoped<IQueryHandler<GetRunescapeAccountHistoryQuery, GetRunescapeAccountHistoryResponse>, GetRunescapeAccountHistoryHandler>();
builder.Services.AddScoped<ICommandHandler<LinkRunescapeAccountRequest>, LinkRunescapeAccountHandler>();
builder.Services.AddScoped<ICommandHandler<DelinkRunescapeAccountRequest>, DelinkRunescapeAccountHandler>();
builder.Services.AddScoped<ICommandHandler<RenameRunescapeAccountRequest>, RenameRunescapeAccountHandler>();

builder.Services.AddScoped<ICommandHandler<AddPointsToUserRequest>, AddPointsToUserHandler>();
builder.Services.AddScoped<ICommandHandler<RemovePointsFromUserRequest>, RemovePointsFromUserHandler>();
builder.Services.AddScoped<IQueryHandler<GetPointHistoryForUserQuery, GetPointHistoryForUserResponse>, GetPointHistoryForUserHandler>();

builder.Services.AddScoped<IQueryHandler<GetRankThresholdsQuery, GetRankThresholdsResponse>, GetRankThresholdsHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateRankThresholdCommand>, UpdateRankThresholdHandler>();

// -- Repositories --

builder.Services.AddSingleton<IUserRepository, PostgresUserRepository>();

// Scoped, matching AppDbContext's own (scoped) lifetime -- same reasoning as
// IRankThresholdProvider above. Note: the dev-admin seeding block below still casts this to
// InMemoryCredentialStore and will throw now that it's Postgres-backed; that cast is fixed by
// issue #49, which also makes this seeding idempotent against a store that persists across
// restarts.
builder.Services.AddScoped<ICredentialStore, PostgresCredentialStore>();

//builder.Services.AddHttpClient<RngHelpdesk.Infrastructure.Discord.HttpDiscordUsernameResolver>(client =>
//{
//    var baseUrl = builder.Configuration["DiscordBot:BaseUrl"] ?? "http://localhost:59854";
//    client.BaseAddress = new Uri(baseUrl);
//});
//builder.Services.AddScoped<RngHelpdesk.Contracts.Discord.IDiscordUsernameResolver>(
//    sp => sp.GetRequiredService<RngHelpdesk.Infrastructure.Discord.HttpDiscordUsernameResolver>());

// -- Projection (Singleton so read models are shared; InMemEventDispatcher captures them) --

builder.Services.AddSingleton<PointHistoryProjection>();
builder.Services.AddSingleton<IPointHistoryReadStore>(sp =>
    sp.GetRequiredService<PointHistoryProjection>());

builder.Services.AddSingleton<UserSummaryProjection>();
builder.Services.AddSingleton<IUserLookupReadStore>(sp =>
    sp.GetRequiredService<UserSummaryProjection>());
builder.Services.AddSingleton<IUserSummaryReadStore>(sp =>
    sp.GetRequiredService<UserSummaryProjection>());

builder.Services.AddSingleton<UserLifecycleHistoryProjection>();
builder.Services.AddSingleton<IUserLifecycleHistoryReadStore>(sp =>
    sp.GetRequiredService<UserLifecycleHistoryProjection>());

builder.Services.AddSingleton<RunescapeAccountHistoryProjection>();
builder.Services.AddSingleton<IRunescapeAccountHistoryReadStore>(sp =>
    sp.GetRequiredService<RunescapeAccountHistoryProjection>());

// -- In-memory event dispatcher --
// Important: dispatcher and ProjectionRunner both receive the exact same singleton
// projection instances that the read-store interfaces resolve to.

static object[] GetProjectionInstances(IServiceProvider sp) => new object[]
{
    sp.GetRequiredService<PointHistoryProjection>(),
    sp.GetRequiredService<UserSummaryProjection>(),
    sp.GetRequiredService<UserLifecycleHistoryProjection>(),
    sp.GetRequiredService<RunescapeAccountHistoryProjection>()
};

builder.Services.AddSingleton<IEventDispatcher>(sp =>
    new InMemEventDispatcher(GetProjectionInstances(sp)));

builder.Services.AddScoped<IProjectionCheckpointStore, PostgresProjectionCheckpointStore>();

builder.Services.AddScoped<ProjectionRunner>(sp =>
    new ProjectionRunner(
        sp.GetRequiredService<IEventStore>(),
        sp.GetRequiredService<EventTypeRegistry>(),
        sp.GetRequiredService<IProjectionCheckpointStore>(),
        GetProjectionInstances(sp)));

// Register the mapped Domain Events -> String names for permanent linkage, even if the classes change over time.
var registry = EventStoreRegistration.CreateRegistry();
builder.Services.AddSingleton(registry);

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

var app = builder.Build();

// Rebuild the in-memory projections from Postgres event history before serving any requests --
// each projection replays from its own last checkpoint (or from 0 if its dictionary came up
// empty, e.g. after a restart).
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<ProjectionRunner>();
    await runner.RunAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();

    var userRepo = scope.ServiceProvider
        .GetRequiredService<IUserRepository>();

    var credentialStore = scope.ServiceProvider
        .GetRequiredService<ICredentialStore>();

    var dispatcher = scope.ServiceProvider
        .GetRequiredService<IEventDispatcher>();

    var roleService = scope.ServiceProvider
        .GetRequiredService<IUserRoleService>();

    const ulong adminId = 123456789012345678UL;

    // The event store (Postgres) persists across restarts, unlike the in-mem credential
    // store below, so only seed the aggregate/role once rather than on every startup.
    if (!await userRepo.ExistsAsync(adminId))
    {
        var user = User.Create(
            actingUserId: adminId,
            discordAccount: new DiscordAccount(
                adminId,
                "admin"),
            runescapeAccounts: []);

        var events = await userRepo.SaveAsync(user);
        dispatcher.Dispatch(events);

        var roleEvents = await roleService.ChangeRoleAsync(
            actingUserId: adminId,
            userId: adminId,
            oldRole: AppRole.Member,
            newRole: AppRole.Owner);

        dispatcher.Dispatch(roleEvents);
    }

    await credentialStore.SeedCredentialsAsync(
        userId: adminId,
        username: "admin",
        password: "password");
}

app.UseHttpsRedirection();

app.UseCors("DevCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();