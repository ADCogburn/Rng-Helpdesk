---
status: accepted
---

# Async infrastructure layer

MVP's `Infrastructure` layer (`IUserRepository`, the read-store interfaces, `ICredentialStore`,
`IRankThresholdProvider`) is synchronous, even though `IEventStore` and the
`ICommandHandler`/`IQueryHandler` interfaces ([ADR 0001](0001-command-query-handler-interfaces.md))
are already async — handler bodies just wrap sync work in `Task.FromResult(...)`. The dormant
`PostgresUserRepository`/`PostgresRankThresholdProvider` already demonstrate the anti-pattern this
decision exists to prevent: forcing async EF/event-store calls through today's sync interfaces via
`.GetAwaiter().GetResult()`. We're porting the rest of `Infrastructure` to be genuinely async ahead
of implementing durable Postgres-backed infrastructure (issue #8), so that work doesn't also have to
fight a signature mismatch.

## Decisions

- New async methods take the `Async` suffix (`GetByIdAsync`, `SaveAsync`, `CreateTemporaryCredentialsAsync`,
  `GetThresholdsAsync`, etc.), matching `IEventStore`/`IUserRoleService.ChangeRoleAsync` rather than
  the bare-name convention `ICommandHandler`/`IQueryHandler.Handle` uses.
- Every new async signature takes `CancellationToken ct = default`, and handler bodies pass through
  the token `Handle(...)` already receives — matching `IEventStore`'s existing pattern, so cancellation
  doesn't force a second breaking signature change once it matters against a real database.
- `out`-param methods (`TryGetById`, `TryGetUserIdsByHistoricalRunescapeUsername`, etc.) become
  nullable-returning async methods (e.g. `Task<UserSummaryReadModel?> GetByIdAsync(...)`), since `out`
  parameters aren't legal on `async` methods.
- `IEventDispatcher.Dispatch` and `IProjectionHandler.Project` stay synchronous — they only ever
  mutate in-memory projection dictionaries, even once the event store and checkpoint store are
  Postgres-backed.
- `CommandHandler.Execute`/`Execute<T>` (sync) are deleted once every handler body is converted to
  `ExecuteAsync`/`ExecuteAsync<T>`, so a future handler can't reach for the sync path and reintroduce
  a blocking call.

## Considered options

- **Tuple return** `Task<(bool Found, T? Value)>` instead of a nullable return — rejected. It stays
  closer to today's `out bool` call sites, but a nullable return is more idiomatic against how EF
  Core's `FindAsync`/`FirstOrDefaultAsync` already read, and no caller needs to distinguish "found a
  null-valued record" from "not found."
- **Splitting into multiple stories** (repository, then read-stores, then credential/rank-provider) —
  rejected. The change applies one consistent pattern uniformly across the layer; splitting it would
  leave the codebase partially-sync/partially-async between PRs and force handlers that touch
  multiple stores to be revisited more than once.
- **Updating the dormant `PostgresUserRepository`/`PostgresRankThresholdProvider` in the same change**
  — rejected, deferred to the separate future "wire up Postgres" work. They aren't registered in DI
  today, so fixing their signatures now adds no compile-time benefit and widens this change's diff.

## Consequences

- Every live Operations handler and its test wiring changes again — a second signature-shape change
  (after ADR 0001) driven by what it now calls into. Mechanical but wide: ~15 handlers, 5 read-store
  interfaces + implementations, `IUserRepository`, `ICredentialStore`, `IRankThresholdProvider`.
- `DeactivateUserHandler`/`ReactivateUserHandler` remain excluded, consistent with ADR 0001's
  existing carve-out for those two.
- `PostgresUserRepository`/`PostgresRankThresholdProvider` are left implementing the *old* sync
  interfaces and won't compile if ever uncommented/registered — whoever picks up the Postgres wiring
  issue needs to update them to the new async shape first.
