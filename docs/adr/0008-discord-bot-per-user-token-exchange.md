---
status: accepted
---

# The Discord bot authenticates as the invoking member via a shared secret and a per-user token exchange

The Discord bot (`RngHelpdesk.DiscordBot`) exposes the helpdesk as guild slash commands (`docs/discord-bot/PLAN.md`). Every command that reads or changes member data has to hit the API as the Discord user who typed it, so that `AdminPlus` checks and `ActingUserId` audit data are exactly what the web UI gets. The bot is a separate process with its own gateway connection, so it needs a way to obtain a normal user JWT for whichever member invoked a command.

## Decisions

**Three-step token flow.** The API exposes two anonymous-or-bot-only endpoints that the bot calls in sequence:

1. `POST auth/bot/token` takes `{apiKey}` and compares it against `DiscordBot:ApiKey` using `CryptographicOperations.FixedTimeEquals`. On a match it issues a bot JWT with a single claim, `client_type=discord_bot`, valid for one hour. It carries no `NameIdentifier` and no `Role`. If `DiscordBot:ApiKey` is unset or blank the endpoint always returns 401, so the feature is off by default.
2. `POST auth/discord` (`[Authorize(Policy = AuthPolicies.DiscordBotOnly)]`) takes `{discordId}`, looks the user up by id, and issues a normal user JWT with `NameIdentifier` and `Role` valid for 15 minutes. It returns 404 when no user has that id and 403 when the user is deactivated. The token deliberately omits `ClaimTypes.Name`, so `auth/change-password` rejects it.
3. The bot calls the ordinary endpoints with that user JWT. `GET auth/me/point-history` was added for member self-service in the same change.

Both tokens are issued by `JwtTokenIssuer` (`Api/Security/`, singleton), which is also used by `auth/login`. Login's claims and lifetime are unchanged.

**The bot JWT has no authority of its own.** It cannot satisfy `AdminPlus` (no role claim) and cannot reach `auth/me` (no `NameIdentifier`, so 401). The only thing it can do is exchange for a user token.

**User id is the Discord snowflake.** `UserCreatedEvent` sets `UserId = discordAccount.DiscordId`, so the bot's `IUser` option id is the API user id and no lookup step is needed.

**Per-user exchange, not a service account.** The bot never acts as a single shared account. Each command runs as the invoking member, so role checks and audit trails attribute every change to the person who made it.

## Considered options

- **One service account for the bot.** Every command would run as the same user. Role checks would be meaningless (everything is one identity) and every audit row would name the bot, not the admin who ran `/points add`. Rejected.
- **Discord OAuth2 login for each member.** Members would have to authorise the app and the API would need redirect URIs and a session store for a bot that already receives the snowflake from the gateway. Heavier than the problem, and it still needs a server-side secret to trust the snowflake. Rejected for now.
- **Members type their helpdesk password into Discord.** Passwords in chat logs, and the bot would hold credentials for every member. Rejected; this is why login and change-password are out of scope for the bot.
- **Trust a bearer token signed by the bot with a separate key pair.** More moving parts (key distribution, rotation) for the same trust boundary as one shared secret that lives only on the API and the bot. Rejected.

## Consequences

The shared secret is powerful. Anyone holding `DiscordBot:ApiKey` can mint a user token for any Discord snowflake in the database, including an admin's, so the key must be treated like a root credential: user-secrets or a deployment secret store only, never in `appsettings.json` or Bruno environments committed to git. Rotating it means changing `DiscordBot:ApiKey` on the API and `Api:BotApiKey` on the bot and restarting both; existing bot and user tokens remain valid until they expire.

The API does not verify Discord identity itself. It takes the bot's word for which snowflake invoked a command, so a compromised bot process is equivalent to a compromised secret. Slash commands are registered to a single guild, which limits who can reach the bot in practice, but the API does not check guild membership.

Token lifetimes are stamped, but `ValidateLifetime = false` is still set in the API's JWT configuration (dev only, see CLAUDE.md), so expiry is not enforced yet. Fixing that flag is what makes the 1-hour and 15-minute lifetimes meaningful; until then they are advisory. Deactivation is checked only at exchange time, so a member deactivated mid-session keeps a working token until it expires.
