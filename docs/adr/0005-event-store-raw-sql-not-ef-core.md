---
status: accepted
---

# Event store uses raw SQL, not EF Core

`AppDbContext`/EF Core is registered and used by `PostgresRankThresholdProvider`/`Repository` and
`PostgresCredentialStore` — ordinary mutable tables (`points.rank_thresholds`, `identity.auth_users`)
that fit EF's tracked-entity model well. `PostgresEventStore` is the one Postgres-backed class that
instead talks to Postgres directly via `NpgsqlDataSource`/raw SQL. `PostgresUserRepository` has no
persistence logic of its own to compare — every method delegates straight to `IEventStore`.

## Why

`eventstore.event_store` doesn't fit the shape EF Core is built for:

- **Polymorphic payload.** The `Payload` column is `JsonSerializer.Serialize(e, e.GetType())` for
  whichever of a dozen-plus event types `e` happens to be at runtime, with a separate `EventType`
  string column as the discriminator. EF's entity mapping expects a fixed shape per table/entity, not
  "one column, runtime-type-driven serialization of an open-ended hierarchy."
- **Concurrency is catch-and-translate, not change-tracking.** `AppendAsync` inserts optimistically
  and relies on the unique constraint on (`StreamType`, `StreamId`, `StreamVersion`) to reject a
  conflicting write; the resulting `PostgresException` is caught and re-thrown as
  `ConcurrencyConflictException`. That's an ADO.NET-level concern — EF's concurrency story
  (`DbUpdateConcurrencyException`, concurrency tokens) is built around `SaveChanges` diffing a
  tracked entity, which doesn't apply here.
- **Append-only.** Every operation is a `select` or a multi-row `insert` inside an explicit
  transaction; nothing is ever updated in place. EF's core value — tracking an entity's mutations and
  diffing them on `SaveChanges` — has nothing to do here.

## Consequences

`PostgresEventStore` will keep looking inconsistent with the rest of `Infrastructure` at a glance —
it's the only class not going through `AppDbContext`. That's deliberate, not an oversight to be
"fixed" by porting it to EF Core; doing so would fight the polymorphic-payload and
catch-and-translate-concurrency shape described above rather than simplify it.
