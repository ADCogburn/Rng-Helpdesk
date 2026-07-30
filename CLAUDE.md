# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

RngHelpdesk is a backend for tracking a Runescape clan's Discord members, their linked
Runescape accounts, and a clan-points/rank system. It's a .NET 10 solution using event
sourcing + CQRS, plus a Discord bot microservice for resolving Discord usernames. There is
currently no working frontend (see "Frontend status" below) — the only thing that actually
runs today is `RngHelpdesk.Api`, tested via Swagger and covered by the test projects below.

## Commands

```powershell
dotnet build RngHelpdesk.slnx                # build everything
dotnet test RngHelpdesk.slnx                 # run all tests
dotnet run --project RngHelpdesk.Api         # run the API (Swagger UI at /swagger in Development)
dotnet run --project RngHelpdesk.DiscordBot  # run the Discord bot (needs Discord:BotToken config)
```

Four test projects exist and are wired into `RngHelpdesk.slnx`: `RngHelpdesk.Domain.Tests`
(unit tests against the `User` aggregate's behavior methods), `RngHelpdesk.Operations.Tests`
(command/query handler tests via a shared `OperationsTestFixture`), `RngHelpdesk.Api.Tests` (controller
tests that manually wire the same in-memory collaborators as `Program.cs` and construct
controllers directly, rather than booting a full HTTP host via `WebApplicationFactory`), and
`RngHelpdesk.Infrastructure.Tests` (integration tests against a real Postgres instance spun up
per test class via `Testcontainers.PostgreSql` — `MigrationFixture` in `MigrationTests.cs` is the
shared `IClassFixture` other Postgres-backed tests key off; requires Docker to run).

`RngHelpdesk.Website` (Angular 17, under `RngHelpdesk.Website/`) is `npm install` + `npm start`
if you need to poke at it, but see "Frontend status" — it's out of sync with the current API.

## Branching model

All feature work branches off `development`, not `master` — PRs target `development`. `master`
is updated from `development` in periodic batches and reflects what's actually deployed to the
production environment; don't target it directly for feature/fix PRs.

## Solution layout (dependency direction)

Strict one-directional Clean/Onion layering — inner layers never reference outer ones:

```
Domain  ←  Contracts  ←  Infrastructure  ←  Operations  ←  Api
```

- **RngHelpdesk.Domain** — the aggregate (`User`) and domain events. No package dependencies.
- **RngHelpdesk.Contracts** — commands/queries/views (the CQRS DTOs) and cross-cutting types
  shared by Operations and Api (ranks, security enums). Depends only on Domain.
- **RngHelpdesk.Infrastructure** — event store, projections/read-stores, repositories, EF Core
  `AppDbContext` + migrations, security/credential storage.
- **RngHelpdesk.Operations** — application-layer command/query handlers that orchestrate
  Domain + Infrastructure.
- **RngHelpdesk.Api** — ASP.NET controllers, JWT auth, FluentValidation, and the full DI
  composition root in `Program.cs`.
- **RngHelpdesk.DiscordBot** — a fully standalone minimal-API microservice (not referenced by
  and doesn't reference any other project). Exposes `GET /discord/users/{discordId}` wrapping
  `Discord.Rest.DiscordRestClient`. The main Api has a `DiscordBot:BaseUrl` config key and
  commented-out `HttpClient`/resolver wiring intended to call this over HTTP, but that
  integration isn't currently connected.
- **RngHelpdesk.Domain.Tests**, **RngHelpdesk.Operations.Tests**, **RngHelpdesk.Api.Tests** —
  test projects paired with the layer they exercise. See "Commands" above.
- **RngHelpdesk.Handlers** — dead/vestigial. No `.csproj`, not in `RngHelpdesk.slnx`, only
  stale `bin`/`obj` build cache left over. Ignore it.

## Event sourcing / CQRS pattern

- `User` (`RngHelpdesk.Domain/Users/User.cs`) is the only aggregate, extending
  `AggregateRoot` (`Domain/Common/AggregateRoot.cs`). Behavior methods validate invariants
  (throwing `DomainException` on violation) then call `RaiseDomainEvent(...)`, which both
  applies the event to in-memory state (via the aggregate's `Apply` switch) and queues it as
  uncommitted. `User.Rehydrate(events)` replays a full event stream via `LoadFromHistory` —
  **there is no snapshotting**, every load replays from the start of the stream.
- Domain events live in `Domain/Users/Events/*` and `Domain/Points/ClanPointsChangedEvent.cs`.
  Each has a static `Create(...)` factory (for domain use, stamping `OccurredAt`); most also have
  a `[JsonConstructor]` ctor for deserializing stored events, but `RunescapeAccountRenamedEvent`
  and `RunescapeAccountDelinkedEvent` currently omit the `[JsonConstructor]` attribute on their
  sole constructor — inconsistent with the rest, not intentional.
- Not everything goes through the aggregate: role changes (`IUserRoleService.ChangeRoleAsync`,
  used by `ChangeUserRoleHandler`) append an `IApplicationEvent`
  (`UserAppRoleChangedEvent`, `Infrastructure/Security/`) directly to the event store, bypassing
  `User` entirely. `IApplicationEvent` vs `IDomainEvent` is the distinction between
  cross-cutting/administrative events and events that mutate the aggregate's own invariants.
- **Handlers** (`RngHelpdesk.Operations/**/*Handler.cs`) follow one of two shapes, both fully
  async:
  - *Command handlers*: `repository.GetByIdAsync(...)` → aggregate behavior method →
    `repository.SaveAsync(user)` (returns the new events) → `eventDispatcher.Dispatch(events)`
    (dispatch itself is still synchronous), all wrapped in `CommandHandler.ExecuteAsync(...)`
    (Contracts/Common) which turns exceptions into a `CommandResult`/`CommandResult<T>`
    (`Success`/`Failure`/`NotFound`).
  - *Query handlers*: read directly from a projection's read-store interface (e.g.
    `IUserSummaryReadStore.GetByIdAsync(...)`), map to a Contracts `View`/`Response` record,
    return `QueryResult<T>`.
  - `LinkRunescapeAccountHandler` is the only handler doing inline FluentValidation before
    executing.
- **Projections** (`RngHelpdesk.Infrastructure/{Users,Points}/*Projection.cs`, plus
  `Infrastructure/Users/RunescapeAccount/RunescapeAccountHistoryProjection.cs`) are singleton,
  dictionary-backed read models. Each implements `IProjectionState` (`IsEmpty`, for detecting a
  restarted/lost in-memory projection) plus `IProjectionHandler<TEvent>` for each event type it
  cares about. `InMemEventDispatcher` (`Infrastructure/Common/`) uses reflection to route each
  dispatched event to every registered projection's matching `Project(TEvent)` method — the
  dispatcher is wired up in `Api/Program.cs` with the exact same singleton instances the
  read-store interfaces resolve to (comment there calls this out explicitly — don't break that
  singleton-sharing when adding a new projection).
- Ranks: `RankResolver` (`Contracts/Common/Ranks/`) takes the resolved `IReadOnlyList<RankThreshold>`
  directly (not the provider itself) and resolves a user's `Rank` from either an admin-tier
  `AppRole` override or their total clan points against sorted thresholds. `IRankThresholdProvider`
  is what supplies those thresholds — it's resolved upstream in `Program.cs` and the result passed
  into `RankResolver`'s constructor.

## Runtime reality: durable event log, ephemeral read side

`RngHelpdesk.Api/Program.cs` is the DI composition root and is the source of truth for what's
actually wired up vs. aspirational. As of now:

- **Durable (Postgres-backed) and live**: `PostgresEventStore` (the event log itself) and
  `PostgresUserRepository` (aggregate persistence/rehydration) — wired live in #44/#45 — plus
  `PostgresRankThresholdProvider` (`points.rank_thresholds`), wired live in #46, and its write-side
  counterpart `PostgresRankThresholdRepository` (`IRankThresholdRepository`, also scoped), wired
  live in #17 behind `RankThresholdsController` (`GET`/`PUT` under `AuthPolicies.AdminPlus`) — the
  only verbs are "read all" and "update `PointsRequired` for an existing `Rank`", since `Rank` is a
  fixed enum with one threshold row per point-based rank; there's no create/delete. `AppDbContext`
  is registered (`AddDbContext` in `Program.cs`), but nothing currently calls `Database.Migrate()`
  at runtime. `PostgresRankThresholdProvider`/`PostgresRankThresholdRepository` are the only
  Postgres-backed classes that actually query through `AppDbContext`/EF Core rather than
  `NpgsqlDataSource`/raw SQL (`PostgresEventStore` and `PostgresUserRepository` still talk to
  Postgres directly). `IRankThresholdProvider` and `IRankThresholdRepository` are both registered
  scoped, matching `AppDbContext`'s own scoped lifetime; the old commented-out
  `CachingRankThresholdProvider` (a singleton-vs-scoped bridge with a manual cache) was dropped as
  part of #46 rather than revived — with `IRankThresholdProvider` itself registered scoped, there's
  no lifetime mismatch left to bridge. `RankResolver` still only reads a point-in-time snapshot of
  thresholds fetched once at startup (via a short-lived `AppDbContext` built ahead of the DI
  container in `Program.cs`), not the live provider, so a threshold row edited via
  `RankThresholdsController` (or directly in the database) won't affect rank resolution until the
  next restart — #17 deliberately left this gap in place rather than fixing it, since the issue's
  scope was the write path itself, not cache invalidation.
  `UpdateRankThresholdHandler` (`Operations/Admin/`) enforces monotonic ordering (a rank's
  threshold must stay strictly between its neighbors') in the handler, using the order
  `IRankThresholdProvider.GetThresholdsAsync` returns thresholds in (ascending `SortOrder`, which
  matches ascending `PointsRequired` for every seeded row).
- **Still in-memory, lost on restart**: `InMemoryCredentialStore` and all four projection read
  models (`UserSummaryProjection`,
  `UserLifecycleHistoryProjection`, `RunescapeAccountHistoryProjection`, `PointHistoryProjection`)
  — plain in-process `Dictionary`s. This is intentional long-term architecture for the
  projections, not a stopgap: there's no Postgres-backed replacement planned for the dictionaries
  themselves, only for the checkpoint that tracks each one's replay position (next point).
  `InMemEventDispatcher` is *also* permanent, despite the naming (tracked in #55) — dispatch is an
  in-process method call, there's nothing to persist, unlike its `InMem*` siblings above.
- **Not wired up**: `PostgresProjectionCheckpointStore` and `ProjectionRunner` are both registered
  only in commented-out code in `Program.cs`, and the startup call to `runner.RunAsync()` is
  likewise commented out. Net effect: the event log now survives a restart but the four
  projections don't get rebuilt from it — after a restart, a projection's dictionary is empty and
  silently stays empty (its `Project()` methods no-op on unknown IDs, no error raised) until this
  wiring is finished. Tracked in #56.
- One EF Core migration exists (`Infrastructure/Migrations/20260729040932_InitAppSchema.cs`,
  schemas: `eventstore`, `projections`, `identity`, `points`).
- In `Development`, `Program.cs` seeds a hardcoded admin user/credentials in-process at startup
  (see the block right after `app.Environment.IsDevelopment()`) — only once now, since the event
  store persists across restarts (see the comment in that block).

When asked to "wire up Postgres" further or "make projections durable," the commented-out
checkpoint-store/`ProjectionRunner` code (#56) is the intended design to uncomment/finish, not a
from-scratch task — read it before rewriting it.

## API layer conventions

- Controllers (`RngHelpdesk.Api/Controllers/`) translate `CommandResult`/`QueryResult` status into
  `Ok`/`NotFound`/`BadRequest`/`NoContent`. Not perfectly consistent: some actions do this via a
  `switch` on `ResultStatus` (e.g. `RunescapeAccountsController`, some of `UsersController`);
  others use an `if (!result.Success)` check instead (e.g. `AdminController`, other `UsersController`
  query actions). Match the existing style in the controller you're editing.
- Auth policies (`Security/AuthPolicies.cs`): `AdminPlus` (Administrator/SuperAdministrator/Owner
  roles), `OwnerOnly`, `DiscordBotOnly` (`client_type` claim). Most controllers apply `AdminPlus`
  at the class level.
- `ClaimsPrincipalExtensions.GetUserId()` (`Api/Helpers/`) is the standard way to pull the acting
  user's ulong ID out of `ClaimTypes.NameIdentifier` in a controller.
- JWT auth: `AuthController` issues tokens (`auth/login`), `Program.cs` configures
  `AddJwtBearer` — note `ValidateLifetime = false` is currently set (marked `// dev only` in
  code, not a mistake to silently "fix").
- FluentValidation is registered globally (`AddFluentValidationAutoValidation` +
  `AddValidatorsFromAssemblyContaining<...>`) but only one validator actually exists today
  (`Validators/Users/LinkRunescapeAccountRequestValidator.cs`).

## Contracts naming conventions (not perfectly consistent — match existing style per-folder, don't "fix" globally)

- Commands: mix of `*Request` (mutable classes or records) and `*Command` (records) suffixes.
- Queries: `*Query` + a paired `*Response`.
- Read-model DTOs returned to callers: `*View` / `*Item`. `*View` types are `sealed record`
  (`RunescapeAccountView`, `DiscordAccountView`); `*Item` types are `sealed class`
  (`PointHistoryItem`, `UserLifecycleHistoryItem`, `RunescapeAccountHistoryItem`) — the suffix
  tracks the record/class split, it isn't mixed within a suffix.
- A few files omit their namespace declaration (sit in the global namespace) inconsistently with
  sibling files in the same folder — this is pre-existing inconsistency, not intentional.

## Frontend status

None of the three frontend-adjacent directories are part of `RngHelpdesk.slnx` or currently
functional:

- **`RngHelpdesk.Website/`** — git-tracked Angular 17 app, proxies `/api` to the API's HTTPS
  port (`src/proxy.conf.js`), but its `api.service.ts` calls a hardcoded `localhost:5000` and a
  `POST /dev/auth/token` endpoint that doesn't exist in the current `AuthController`. Out of
  sync with the current API — treat as legacy/reference only.
- **`RngHelpdesk.Web/`** — referenced by `RngHelpdesk.slnLaunch.user` as the intended SPA host
  project, but `RngHelpdesk.Web.csproj` doesn't exist on disk and the directory is untracked in
  git. Not currently buildable.
- **`web/`** — appears to be a newer scaffold attempt but has no `package.json`, no app code, and
  is untracked in git. Effectively empty.

If asked to build or fix "the frontend," clarify which of these three the user means before
assuming — none of them is an obvious default, and building on top of any of them may mean
finishing scaffolding first.

## Maintaining this file

Future sessions trust this file as source of truth — e.g. "Runtime reality" above explicitly
tells agents to read it before rewriting `Program.cs` wiring, rather than re-deriving what's live
from scratch. A stale claim here doesn't just go unnoticed, it actively misleads whoever reads it
next (this happened: the "Runtime reality" section still described `PostgresEventStore` and
`PostgresUserRepository` as commented-out well after #44/#45 wired them live). If a PR changes
something this file describes — wires up a class documented here as commented-out, renames or
removes something referenced here, changes a convention documented here — update the relevant
section in the same PR instead of leaving it for a future session to rediscover the drift.

## Agent skills

### Issue tracker

Issues live in GitHub Issues for `ADCogburn/Rng-Helpdesk`, using the `gh` CLI. See `docs/agents/issue-tracker.md`.

### Domain docs

Single-context layout — `CONTEXT.md` + `docs/adr/` at the repo root. See `docs/agents/domain.md`.
