---
status: accepted
---

# ulong ids are JSON strings API-wide; the landing page reads anonymous `/public` endpoints

Built alongside the `web/` React frontend (`docs/ui/PLAN.md`). Two API-contract decisions it forced, recorded together because both shape what crosses the wire.

User ids are Discord snowflakes (`ulong`), and they routinely exceed JavaScript's `Number.MAX_SAFE_INTEGER` (2^53 − 1). Serialized as JSON numbers, `JSON.parse` silently rounds them — the client then sends back a *different* id and operates on the wrong user (or none). Separately, the public landing page needs live clan stats, a rank ladder and a leaderboard without a login, but the existing read endpoints are all `AdminPlus` and return Discord ids and usernames.

## Decisions

**ulong as string.** `UInt64StringJsonConverter` (`Api/Serialization/`) is registered in `Program.cs`'s JSON options next to `JsonStringEnumConverter`, so every `ulong` the API writes — response ids, `discordId`, dictionary keys — is a JSON string. On read it accepts a string *or* a number, so existing callers (Bruno requests, older clients, tests) that send numeric ids keep working. Nullable `ulong` is covered by `System.Text.Json`'s nullable wrapper. Route-bound ids are unaffected (they're already strings in the URL).

**Anonymous `/public/*` endpoints.** `PublicController` (`[AllowAnonymous]`, read-only) serves `GET /public/overview` (active member count, total clan points, members per point-based rank), `GET /public/ranks` (the ladder, ascending by points required) and `GET /public/leaderboard?top=` (clamped 1..100, default 25). They expose only RSN, rank and points aggregates — no user ids, no Discord ids or usernames. The leaderboard is limited to active members with at least one linked RSN (the first RSN is shown), ordered by points descending then RSN; overview counts active users only. They read the same projections and `IRankThresholdProvider` as the admin queries via dedicated query handlers, so no new persistence exists.

## Considered options

- **Keep numeric ids and parse JSON losslessly in the client** (e.g. a custom parser or `json-bigint`) — rejected. It would push the fix into every consumer, including future ones, defeats `fetch().json()` and ordinary tooling (Swagger UI, browser devtools, Bruno's JS) which all corrupt the value, and the failure mode is silent.
- **Per-property `[JsonNumberHandling(WriteAsString)]` / `[JsonConverter]` attributes on each `ulong`** — rejected. There are many contract types carrying ids; any one forgotten reintroduces silent corruption, and the attributes would sit on Contracts/Domain types that shouldn't know about JSON wire concerns. A single global converter covers new types automatically.
- **Reuse the `AdminPlus` read endpoints for the landing page via a service account or by relaxing their auth** — rejected. They return Discord data and every member; a public surface should be a separate, deliberately minimal contract rather than a loosened private one.

## Consequences

Any client — including tests asserting on serialized JSON — sees ids as strings; `web/src/api/types.ts` types them `string` and never converts to `number`. The converter's read-side leniency means numeric ids in request bodies remain valid, but a numeric id that was already corrupted before reaching the API can't be detected. Public data is intentionally lossy (no ids), so the landing page can't link a leaderboard row to a profile; and because the public queries are cheap projection reads that anyone can hit, they have no rate limiting today. Adding a field to a `/public` response is a privacy decision — check that it isn't an id or Discord data first.
