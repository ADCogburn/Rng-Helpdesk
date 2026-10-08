# Discord bot plan — slash-command front end for the RngHelpdesk API

Branch: `feature/discord-bot` (off `origin/development`, PR targets `development`).
Orchestrated from a Herdr pane; tasks below are executed by worker agents (Haiku/Sonnet) and
reviewed + committed by the orchestrator. **Workers never commit, push, or open PRs.**

## Goals

1. A Discord bot (`RngHelpdesk.DiscordBot`) that exposes, as guild slash commands, every API
   capability that makes sense to do from Discord: public clan stats, a member's own profile and
   points, and the full admin toolkit (members, points, RSNs, roles, rank thresholds).
2. Authorization stays in the API. The bot acts **as the Discord user who ran the command**, so
   `AdminPlus` checks and `ActingUserId` audit data are exactly what the web UI gets.

Out of scope (deliberately **not** bot commands): login, change-password and anything else that
involves typing a password into Discord; rank-up/announcement push notifications (needs an API →
bot event channel that doesn't exist); multi-guild support; the commented-out
`HttpDiscordUsernameResolver` wiring in the API's `Program.cs` (#13/#14 territory).

## Decisions (made with the user, 2026-10-08)

- **Per-user token exchange.** The bot holds a shared secret (`DiscordBot:ApiKey` on the API,
  `Api:BotApiKey` on the bot). It trades the secret for a *bot* JWT (`client_type=discord_bot`), then
  trades that + the invoking user's Discord snowflake for a short-lived *user* JWT, and calls the
  normal endpoints with it. This is the flow the existing comment in `AuthController.Login` and the
  unused `AuthPolicies.DiscordBotOnly` policy were reserved for.
- **User id == Discord snowflake** (`UserCreatedEvent` sets `UserId = discordAccount.DiscordId`), so a
  slash-command `IUser` option's `Id` is the API user id directly. No lookup step.
- `/member add` shows the generated temporary credentials **ephemerally to the admin only** (mirrors
  the web UI's one-time panel). No DMs.
- New member self-service endpoint **`GET auth/me/point-history`** so members can see their own
  history from Discord.
- Slash commands are registered to **one guild** (`Discord:GuildId`) — instant updates, single clan.
- The bot stays **standalone**: it does *not* reference `RngHelpdesk.Contracts` or any other project.
  It is an HTTP client of the API, like `web/`, with its own DTOs (ids as `ulong` on the C# side,
  `string` on the wire — needs its own ulong-as-string converter, mirroring ADR-0007).
- No Discord-permission gating on admin commands. Every member can *see* them; the API's 403 is
  turned into a friendly ephemeral "you need an admin role" reply. (Discord's
  `DefaultMemberPermissions` can't express "has an app role in our database".)
- All admin replies and all error replies are **ephemeral**. Public read commands (`/leaderboard`,
  `/ranks`, `/clan`) reply publicly; `/profile` and `/points mine` reply ephemerally.

## API additions (tasks A1, A2)

| Method | Route | Auth | Body → Response |
|---|---|---|---|
| POST | `/auth/bot/token` | anon (shared secret in body) | `{apiKey}` → `{token, expiresAt}` — JWT with only `client_type=discord_bot` (no `NameIdentifier`, no `Role`), 1 h lifetime. 401 on wrong key; **401 always if `DiscordBot:ApiKey` is unset/blank** (feature off by default). Constant-time comparison (`CryptographicOperations.FixedTimeEquals`). |
| POST | `/auth/discord` | `DiscordBotOnly` | `{discordId}` (string on the wire) → `{token, expiresAt, appRole}` — a normal user JWT (`NameIdentifier`, `Role`; **no `ClaimTypes.Name`**, so `change-password` rejects it) with a 15 min lifetime. 404 if no user has that id; 403 if the user is deactivated. |
| GET | `/auth/me/point-history` | any user JWT | → `GetPointHistoryForUserResponse` for the caller (reuses `GetPointHistoryForUserHandler`). 401 without a `NameIdentifier`. |

Notes:
- Extract token creation from `AuthController.Login` into a small `JwtTokenIssuer`
  (`Api/Security/`, registered singleton, reads `Jwt:*` config) used by login, bot token and exchange.
  `Login` behaviour and claims must not change.
- A bot JWT must not satisfy `AdminPlus` (it has no role claim) and must not work on `auth/me`
  (no `NameIdentifier` → 401). Keep it that way.
- `ValidateLifetime = false` is still set (dev only, per CLAUDE.md) — do not change it; the expiry is
  still stamped so it starts mattering when that flag is fixed.
- Document the flow in **ADR-0008** (task D1).

## Existing API surface the bot uses

See the contract table in `docs/ui/PLAN.md` ("API contract"). Summary of shapes the bot needs:
`GetUserResponse = {id, appRole, clanPoints, rank, isActive, dateCreated,
discordAccount:{discordId,username}, runescapeAccounts:[{username}]}`; ids are JSON strings; enums are
strings. Errors: `400` plain string or ValidationProblemDetails; `404` plain string; `401`/`403` empty.
RSN rule: `^(?! )[A-Za-z0-9 -]{1,12}(?<! )$`.

## Command map

`@user` = a Discord user option (its `Id` is the API user id). "A+" = API enforces `AdminPlus`.

| Command | Who | API calls | Reply |
|---|---|---|---|
| `/clan` | anyone | `GET public/overview` | public embed: active members, total points, rank distribution |
| `/ranks` | anyone | `GET public/ranks` | public embed: the 14-tier ladder with points required |
| `/leaderboard [top=10]` | anyone | `GET public/leaderboard?top=` (bot clamps 1..25) | public embed, medals for top 3 |
| `/profile` | registered member | `GET auth/me` + `GET public/ranks` | ephemeral embed: rank (rank colour), points, progress bar to next rank, RSNs, role |
| `/points mine` | registered member | `GET auth/me/point-history` | ephemeral embed: last 10 changes (delta, reason, date, rank change) |
| `/member info @user` | A+ | `GET users/{id}` | profile embed incl. Discord, role, active state, created date |
| `/member find rsn:` | A+ | `GET users/by-rsn/{rsn}`; on 404 `GET users/by-historical-rsn/{rsn}` | current owner, or "previously used by" list |
| `/member add @user [rsn1] [rsn2] [rsn3]` | A+ | `POST admin/create` (`discordAccount.username` = the user's Discord `Username`) | ephemeral: created + login username + temporary password + "share this privately, shown once" |
| `/member deactivate @user` | A+ | confirm button → `POST admin/{id}/deactivate` | ephemeral |
| `/member reactivate @user` | A+ | `POST admin/{id}/reactivate` | ephemeral |
| `/member lifecycle @user` | A+ | `GET users/{id}/lifecycle` | timeline embed |
| `/role promote @user` | A+ | confirm button → `POST admin/{id}/promote` | ephemeral |
| `/role demote @user` | A+ | confirm button → `POST admin/{id}/demote` | ephemeral |
| `/points add @user amount reason` | A+ | `POST users/{id}/points/add` then `GET users/{id}` | ephemeral: new total + rank |
| `/points remove @user amount reason` | A+ | `POST users/{id}/points/remove` then `GET users/{id}` | ephemeral: new total + rank |
| `/points history @user` | A+ | `GET users/{id}/point-history` | last 10 changes |
| `/rsn list @user` | A+ | `GET users/{id}/runescape-accounts` + `.../previous` | current + previous RSNs |
| `/rsn link @user rsn` | A+ | `POST users/{id}/runescape-accounts` | ephemeral |
| `/rsn delink @user rsn` | A+ | `DELETE users/{id}/runescape-accounts` (JSON body) | ephemeral; `rsn` autocompletes from the user's current RSNs |
| `/rsn rename @user old new` | A+ | `PUT users/{id}/runescape-accounts/rename` | ephemeral; `old` autocompletes like delink |
| `/rsn history @user` | A+ | `GET users/{id}/runescape-accounts/history` | timeline embed |
| `/thresholds view` | A+ | `GET rankthresholds` | ephemeral table |
| `/thresholds set rank points` | A+ | `PUT rankthresholds/{rank}` | ephemeral + reminder "rank resolution picks this up after an API restart" (documented gap) |

Self-action guard (mirrors the web UI): `/member deactivate`, `/role promote|demote` targeting
yourself are refused by the bot before calling the API.

Error mapping (one place, `InteractionErrorHandler` or similar): exchange 404 → "You're not
registered with the clan helpdesk yet — ask an admin to run `/member add` for you."; exchange 403 →
"Your helpdesk account is deactivated."; 403 from an A+ endpoint → "You need an admin role in the
clan helpdesk to do that."; 404 → the API's message or "Member not found."; 400 → the API's message
(flatten ValidationProblemDetails field errors); network failure/5xx → "The helpdesk API is
unavailable, try again later." (logged). Always ephemeral.

## Bot architecture (`RngHelpdesk.DiscordBot/`)

Keep it a `WebApplication` so the existing `GET /discord/users/{discordId}` resolver endpoint keeps
working, but back it with the gateway client's `Rest` client instead of a second login.

```
RngHelpdesk.DiscordBot/
  Program.cs                      composition root (options, HttpClient, Discord client, hosted service, resolver endpoint)
  BotOptions.cs                   Discord:{BotToken,GuildId}, Api:{BaseUrl,BotApiKey}  (validated on start)
  DiscordBotService.cs            IHostedService: DiscordSocketClient login/start, InteractionService
                                  module discovery (AddModulesAsync), RegisterCommandsToGuildAsync on Ready,
                                  InteractionCreated → ExecuteCommandAsync with a scoped IServiceProvider
  Api/
    RngApiClient.cs               typed HttpClient, one method per endpoint the bot uses; takes the acting
                                  Discord id (or none for /public/*); throws ApiException(status, message)
    ApiTokenService.cs            caches the bot JWT; exchanges + caches user JWTs per Discord id (refresh
                                  1 min before expiresAt); on a 401 from a normal call, drop the cached
                                  token and retry once
    ApiException.cs
    Models/*.cs                   DTO records mirroring the contract; enums Rank, AppRole
    Serialization/UInt64StringJsonConverter.cs  (same semantics as the API's: write string, read string|number)
  Formatting/
    Embeds.cs                     pure functions DTO → Embed/strings (profile, leaderboard, ladder, history…)
    RankStyle.cs                  rank → Discord Color + emoji; progress-to-next-rank calculation
  Interactions/
    ErrorReplies.cs               ApiException/HttpRequestException → friendly message (see Error mapping)
    PublicModule.cs, ProfileModule.cs, MemberModule.cs, RoleModule.cs, PointsModule.cs,
    RsnModule.cs, ThresholdsModule.cs, Autocomplete/*.cs, Confirm components
  README.md                       Discord application setup (see D1)
RngHelpdesk.DiscordBot.Tests/     xUnit, same package versions as RngHelpdesk.Api.Tests
```

Library: **Discord.Net** (already referenced, 3.17.0 — bump to the latest 3.x if available) using
`Discord.WebSocket` + `Discord.Interactions`. Gateway intents: `Guilds` only (no privileged intents;
slash commands don't need message content or members). JSON on the bot side:
`JsonSerializerDefaults.Web` + `JsonStringEnumConverter` + the ulong converter.

Testing: modules are thin glue and aren't unit tested. `RngApiClient`/`ApiTokenService` are tested
with a fake `HttpMessageHandler` (token acquisition + caching + 401 retry, ids round-trip as strings,
error-status → `ApiException`); `Formatting` and `ErrorReplies` are tested as pure functions.

Config: `appsettings.json` gets blank `Discord:GuildId`, `Api:BaseUrl` (`https://localhost:5081`),
`Api:BotApiKey`. Secrets via `dotnet user-secrets` (add a `UserSecretsId`) — never committed. In
Development only, the API `HttpClient` may skip TLS validation for `localhost` (dev cert), mirroring
the Vite proxy's `secure: false`; never in other environments.

## Tasks

Model: **H** = Haiku, **S** = Sonnet. "Done when" = acceptance criteria the orchestrator checks.
Every task: `dotnet build RngHelpdesk.slnx` with no new warnings + `dotnet test` for touched test
projects green. Workers report in `<scratchpad>/reports/<task>.md` (what changed, how verified, open
questions) and never commit.

### API lane (serial — shares `AuthController`, `Program.cs`, `ApiTestFixture`)

- **A1 (S) — bot auth.** `JwtTokenIssuer`; `POST auth/bot/token`; `POST auth/discord`
  (`DiscordBotOnly`); DTOs in `Api/DTOs/`; `DiscordBot:ApiKey` read from config (blank in committed
  `appsettings.json`). Api.Tests: wrong/blank key → 401, right key → token carries
  `client_type=discord_bot` and no role/name-identifier; exchange: unknown id → 404, deactivated → 403,
  active → token with the user's `NameIdentifier` + `Role` and no `Name`; `Login` tests still green.
  Bruno: `Auth/Bot Token` (stashes `botToken`) and `Auth/Discord Exchange` (uses `botToken`, sends
  `discordId` as a string), plus a `botApiKey` variable in `environments/Local.bru` (blank).
- **A2 (H) — `GET auth/me/point-history`.** After A1. Api.Tests for 200 and 401; Bruno request.

### Bot lane

- **K1 (S) — bot host + API client foundation.** Everything under "Bot architecture" except
  `Formatting/` and the command modules: options + validation, `DiscordBotService`, resolver endpoint
  rebased on the gateway client, `Api/*` (client methods for **every** row of the command map, the
  three new auth endpoints included, DTOs, converter, token service), `ErrorReplies`, one trivial
  `/ping` command proving the pipeline, `RngHelpdesk.DiscordBot.Tests` project added to
  `RngHelpdesk.slnx` with the client/token/error tests described above. Can run in parallel with A1
  (contract above is the spec). Done when: build + tests green; the bot starts, fails fast with a
  clear message on missing config.
- **K2 (H) — formatting.** `Formatting/Embeds.cs` + `RankStyle.cs` and tests. Rank colours from
  `docs/ui/PLAN.md` "Rank colours" (Administrator/DeputyOwner/Owner → gold `#d4a84b`). Progress
  calc: next point-based rank above current points, or "max rank" at Zenyte / role-based ranks.
  Depends K1 (DTOs).
- **K3 (H) — public + self commands.** `/clan`, `/ranks`, `/leaderboard`, `/profile`, `/points mine`.
  Depends K2, A2. (`/points mine` lives in the `/points` group — coordinate: K3 creates
  `PointsModule` with the group and `mine`; K5 adds the admin subcommands.)
- **K4 (S) — member + role commands.** `/member *`, `/role *`, confirm-button component flow
  (custom ids carry action + target id; only the invoking user can press; 60 s timeout →
  buttons disabled), self-action guard. Depends K2.
- **K5 (H) — points + RSN commands.** Admin `/points add|remove|history`, `/rsn *` with autocomplete
  handler for current RSNs. Depends K3 (PointsModule exists), K2.
- **K6 (H) — thresholds commands.** `/thresholds view|set` (rank option = choice of the 14
  point-based ranks). Depends K2. Parallel with K4/K5.

### Wrap-up

- **D1 (H) — docs.** ADR-0008 (Discord bot auth: shared secret → bot JWT → per-user token exchange;
  alternatives considered: single service account); `RngHelpdesk.DiscordBot/README.md` (create the
  Discord application, bot token, invite URL with scopes `bot applications.commands` and no
  privileged intents, guild id, user-secrets for both projects, run order); CLAUDE.md (Solution layout
  entry for the bot is no longer "only resolver", new test project, auth endpoints in "API layer
  conventions", commands to run the bot); this file's status table.
- **Q1 (orchestrator) — end-to-end.** API + local Postgres + bot against a real test guild (needs a
  bot token + guild id from the user); run every command once as admin and as a non-registered user;
  fix-ups dispatched to workers; then PR to `development`.

## Status

| Task | Model | State |
|---|---|---|
| A1 | S | todo |
| A2 | H | todo |
| K1 | S | todo |
| K2 | H | todo |
| K3 | H | todo |
| K4 | S | todo |
| K5 | H | todo |
| K6 | H | todo |
| D1 | H | todo |
| Q1 | — | todo |
