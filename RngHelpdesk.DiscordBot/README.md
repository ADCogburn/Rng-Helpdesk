# RngHelpdesk.DiscordBot

Discord slash-command front end for the RngHelpdesk API. Members use it for public clan stats and their
own profile and points; admins get the member, role, points, RSN and rank-threshold toolkit. The bot
holds no authority of its own: every command runs as the Discord user who invoked it, and the API decides
what they're allowed to do. See [ADR 0008](../docs/adr/0008-discord-bot-per-user-token-exchange.md) for the
auth flow.

The bot is standalone. It doesn't reference any other project and talks to the API over HTTP only.

## 1. Create the Discord application

1. Open the [Discord Developer Portal](https://discord.com/developers/applications) and choose **New Application**.
2. **Bot** tab: click **Reset Token** and copy the token. It's shown once; this is `Discord:BotToken`.
   - Leave every **Privileged Gateway Intent** switched off. The bot only receives guild-level
     events for slash commands, so it doesn't need message content or member intents.
3. **OAuth2 → URL Generator**: tick the scopes `bot` and `applications.commands`. No bot permissions
   are needed, since every reply is an interaction response. Open the generated URL and invite the
   bot to your server.

## 2. Find the guild id

1. In Discord, open **User Settings → Advanced** and turn on **Developer Mode**.
2. Right-click the server icon and choose **Copy Server ID**. This is `Discord:GuildId`.

Commands are registered to that one guild, so they appear instantly.

## 3. Configuration

Settings live in user-secrets, never in committed files. From the repository root:

```powershell
# Bot (Discord + how to reach the API)
dotnet user-secrets set "Discord:BotToken" "<bot token>" --project RngHelpdesk.DiscordBot
dotnet user-secrets set "Discord:GuildId" "<server id>" --project RngHelpdesk.DiscordBot
dotnet user-secrets set "Api:BotApiKey" "<shared secret>" --project RngHelpdesk.DiscordBot

# API (the same shared secret, so it can mint bot tokens)
dotnet user-secrets set "DiscordBot:ApiKey" "<shared secret>" --project RngHelpdesk.Api
```

Generate the shared secret once and use the same value for both. Any long random string works, for example
`(New-Guid).Guid + (New-Guid).Guid` in PowerShell. Leaving `DiscordBot:ApiKey` unset keeps the bot-token
endpoint disabled, so the feature is off by default.

| Key | Where | Meaning |
|---|---|---|
| `Discord:BotToken` | bot (secret) | Token from the Developer Portal. |
| `Discord:GuildId` | bot (secret) | Numeric id of the guild to register commands in. |
| `Api:BaseUrl` | bot (`appsettings.json`) | Base URL of the API. Defaults to `https://localhost:5081`. |
| `Api:BotApiKey` | bot (secret) | Must equal `DiscordBot:ApiKey` on the API. |
| `DiscordBot:ApiKey` | API (secret) | Shared secret the API checks in `auth/bot/token`. |

The committed `appsettings.json` holds blank placeholders plus the default `Api:BaseUrl`. The bot
checks these on startup and exits with a message listing every missing or invalid setting.

**Watch out for `appsettings.Development.json`.** It's gitignored and loaded on top of `appsettings.json`,
so any value in it wins over the committed file. A stale `Api:BaseUrl` left there from an earlier
setup will silently point the bot at the wrong place. User-secrets override both files, so if a value
looks wrong, check that file first and delete the stale key.

The bot's HTTP client skips certificate validation for `localhost` in `Development` only (the API's dev
certificate), mirroring the web frontend's Vite proxy. It's never disabled in other environments. If the
API is on HTTPS, trust its dev certificate once with `dotnet dev-certs https --trust`.

## 4. Run order

Start the three pieces in this order:

1. **Postgres**: `podman compose up -d` (or `docker compose up -d`) from the repository root.
2. **API**: `dotnet run --project RngHelpdesk.Api`, or the VS Code `http` profile. It listens on
   `https://localhost:5081` (and `http://localhost:5080`).
3. **Bot**: `dotnet run --project RngHelpdesk.DiscordBot`. It logs in to the gateway, discovers the
   modules, and registers the commands to your guild once ready.

The bot also serves `GET /discord/users/{discordId}` on `http://localhost:59854`, a username resolver
kept for the API's (currently disconnected) Discord username lookup. Nothing in the bot's commands uses it.

If a command replies "The helpdesk API is unavailable", check that the API is running and that
`Api:BaseUrl` points at it.

## 5. Commands

Replies are ephemeral unless noted. "Admin" means the API enforces `AdminPlus`; the bot doesn't check roles,
so non-admins see the command and get "You need an admin role in the clan helpdesk to do that." when they
run it.

| Command | Who | What it does |
|---|---|---|
| `/ping` | anyone | Checks the bot is alive and reports gateway latency. |
| `/clan` | anyone (public) | Active member count, total points and rank distribution. |
| `/ranks` | anyone (public) | The rank ladder with points required for each tier. |
| `/leaderboard [top]` | anyone (public) | Top members by points (1–25, default 10), medals for the top three. |
| `/profile` | registered member | Your rank, points, progress to the next rank and linked RSNs. |
| `/points mine` | registered member | Your last 10 point changes. |
| `/points add user amount reason` | admin | Adds points to a member and shows their new total and rank. |
| `/points remove user amount reason` | admin | Removes points from a member and shows their new total and rank. |
| `/points history user` | admin | A member's last 10 point changes. |
| `/member info user` | admin | A member's profile, including Discord account, role and active state. |
| `/member find rsn` | admin | Who currently owns an RSN, or who previously used it. |
| `/member add user [rsn1] [rsn2] [rsn3]` | admin | Registers a Discord user. Shows the generated temporary password to you only, once. |
| `/member deactivate user` | admin | Deactivates a member. Asks for confirmation. |
| `/member reactivate user` | admin | Reactivates a deactivated member. |
| `/member lifecycle user` | admin | A member's account lifecycle timeline. |
| `/role promote user` | admin | Promotes a member to Administrator. Asks for confirmation. |
| `/role demote user` | admin | Demotes an administrator to Member. Asks for confirmation. |
| `/rsn list user` | admin | A member's current and previous RuneScape names. |
| `/rsn link user rsn` | admin | Links a RuneScape name to a member. |
| `/rsn delink user rsn` | admin | Removes a RuneScape name from a member. `rsn` autocompletes from their current names. |
| `/rsn rename user old new` | admin | Renames one of a member's RuneScape names. `old` autocompletes the same way. |
| `/rsn history user` | admin | A member's RuneScape name change history. |
| `/thresholds view` | admin | The current points required for each rank. |
| `/thresholds set rank points` | admin | Changes the points required for a point-based rank. |

`/thresholds set` reminds you that rank resolution picks the change up after an API restart. That's a known
gap: thresholds are read once at startup (see `CLAUDE.md`, "Runtime reality").

Self-action guard: `/member deactivate`, `/role promote` and `/role demote` refuse to act on yourself.

## 6. Tests

```powershell
dotnet test RngHelpdesk.DiscordBot.Tests
```

The tests cover the API client and token service (with a fake `HttpMessageHandler`), error mapping, bot
options validation, and the formatting functions. The command modules are thin glue and aren't unit tested;
they're checked end to end against a real guild.
