---
status: accepted
---

# Postgres persistence layer

Everything in `Infrastructure` is in-memory today; state is lost on every restart. A prior pass
(dormant `PostgresEventStore`, `PostgresUserRepository`, `PostgresRankThresholdProvider`,
`CachingRankThresholdProvider`) attempted to wire up Postgres but was left entirely commented out
and, per [ADR 0002](0002-async-infrastructure-layer.md), targets the old sync/`int`-keyed
interfaces from before that ADR's async conversion — it won't compile if uncommented as-is.
`PostgresProjectionCheckpointStore` and `ProjectionRunner` do compile against current interfaces
but are unregistered. No Postgres-backed `ICredentialStore` exists at all, not even a stub. We're
replacing all of this with a genuinely wired-up Postgres persistence layer rather than continuing
to carry it as dead code behind comments.

## Decisions

- **Rewrite, don't repair.** CLAUDE.md's stated default is to finish the dormant code, not
  rewrite it — we're deliberately deviating here. The old logic (version-locked reads, per-event
  checkpointing) isn't distrusted on its own terms; patching ~12 layered mismatches (stale
  interfaces, a nonexistent `IEventStoreMetadataProvider` type, `int` vs `ulong` keys throughout)
  was judged more error-prone than a clean pass against the current interfaces. CLAUDE.md's
  "read it before rewriting it" guidance needs updating to reflect this — tracked as a separate
  GitHub issue, not fixed in this change.
- **Scope covers the full stack**: `PostgresEventStore`, a rewritten `PostgresUserRepository`,
  `PostgresRankThresholdProvider`, `PostgresProjectionCheckpointStore` + `ProjectionRunner`
  wiring, and a new `PostgresCredentialStore` (no prior implementation existed for this one).
- **Event store concurrency** relies on the existing unique constraint on
  (`StreamType`, `StreamId`, `StreamVersion`) — insert optimistically, translate a unique-violation
  into a concurrency-conflict exception — rather than the old approach's explicit
  `SELECT ... FOR UPDATE` row lock. Simpler, and the constraint already does the enforcement.
- **`StreamId` becomes `bigint`**, not the migration's original `int`. Domain IDs are `ulong`
  Discord snowflakes, which routinely exceed `int.MaxValue`; the old schema would have silently
  truncated them.
- **Identity schema is narrowed to what `ICredentialStore` actually needs.** The original
  migration scaffolded a generalized `actor_user_links` (`ActorId`/`ActorType` → `UserId`) table,
  presumably for a future multi-auth-method design (e.g. Discord OAuth as a second actor type).
  Nothing in the codebase consumes that concept today — `ICredentialStore` only knows
  username/password. We're dropping `actor_user_links` and persisting only `auth_users`
  (username, password hash, user ID, must-change-password flag), matching
  `InMemoryCredentialStore`'s actual shape. This doesn't preclude adding a generalized actor
  model later if/when OAuth becomes real scope.
- **Rank thresholds**: `InMemoryRankThresholdProvider`'s values (e.g. `Iron=10`, `Zenyte=5000`)
  are canonical. The migration's seed data (`Iron=100`, `Zenyte=250000` — off by 10–50x
  depending on rank) was stale placeholder data and is corrected to match on the rewrite.
- **The existing migration is replaced, not layered on top of.** `InitAppSchema` has never been
  applied to any real database (nothing calls `Database.Migrate()` today), so there's no
  migration history worth preserving — a fresh migration is generated from the corrected model
  instead of adding a second corrective one.
- **Migration application**: `Database.Migrate()` runs automatically at startup only in
  `Development`, matching the existing environment-branching pattern for dev seeding. Staging and
  Production require an explicit `dotnet ef database update` step — auto-migrate-on-boot is
  avoided there.
- **Development now runs against Postgres too**, not just Staging/Production — otherwise the
  Postgres path never gets exercised day-to-day, and behavior would silently diverge between what
  engineers see locally and what's deployed. This means the dev-admin seeding block in
  `Program.cs`, which currently casts `IUserRepository`/`ICredentialStore` directly to their
  in-memory concrete types, has to be rewritten to seed through the interfaces themselves
  (idempotent — checked via `ExistsAsync` — so it's safe against a Postgres store that persists
  across restarts).
- **Local Postgres is provisioned via a new `docker-compose.yml`**, written to be Podman-compatible
  (no Docker-specific compose extensions) since Podman is the assumed runtime in all environments,
  not just a Docker convenience wrapper.
- **Connection strings and secrets move to `dotnet user-secrets`** for Development, out of
  `appsettings.Development.json` — which currently has a real Postgres password and an apparent
  Discord bot secret committed in plaintext. The already-exposed password should be rotated since
  it's already in git history.
- **`ProjectionRunner` gets per-projection failure isolation.** The old version had no try/catch
  around a projection's handling of an event — one broken projection handler would throw and
  abort replay for every other projection too. The rewrite wraps each projection's event handling
  independently so one bad combination doesn't block the rest.
- **A new `RngHelpdesk.Infrastructure.Tests` project is added**, using Testcontainers against a
  real ephemeral Postgres instance, to cover the event store's concurrency-conflict behavior and
  the migration itself — the kind of code where "looks right" and "is right" diverge, and reading
  the code isn't enough to trust it.

## Considered options

- **Repair the dormant classes' signatures in place** instead of rewriting — rejected. Not
  because the old design was wrong, but because patching the accumulated interface drift was
  judged more error-prone than writing once against current interfaces.
- **Keep `actor_user_links` for future OAuth support** — rejected as speculative; nothing in the
  codebase has an OAuth or multi-actor concept today, and building schema for it now is designing
  ahead of actual scope.
- **Explicit `SELECT ... FOR UPDATE` locking** for event append concurrency — rejected in favor of
  relying on the unique constraint; simpler and the constraint already exists.
- **Layering a corrective migration on top of `InitAppSchema`** — rejected since the original
  migration was never applied anywhere; nothing to preserve.

## Consequences

- Local dev now requires a running Postgres (via `podman compose up`) instead of the current
  zero-setup in-memory loop.
- Staging/Production deploys need an explicit migration-apply step added to whatever deploy
  process eventually exists — not yet defined elsewhere, since nothing has deployed this project
  before.
- `InMemoryRankThresholdProvider`'s values become the permanent source of truth for rank
  thresholds going forward; if the real clan point values differ from what's in that file today,
  this locks in the wrong numbers until someone notices and corrects them explicitly.
- CLAUDE.md's "Runtime reality: everything is in-memory today" section and its "read it before
  rewriting it" instruction both go stale the moment this lands — tracked as a follow-up issue,
  not fixed here.
