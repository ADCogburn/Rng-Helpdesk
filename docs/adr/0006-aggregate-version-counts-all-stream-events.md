---
status: accepted
---

# Aggregate version counts every stream event, not just applied ones

Found while building the `ChangeUserRoleHandler` Postgres integration test from [ADR 0004](0004-two-tier-operations-handler-testing.md): `UserAppRoleChangedEvent` (an `IApplicationEvent`) is appended to the same `"User"` stream `PostgresUserRepository` uses for `User`'s own `IDomainEvent`s (`UserRoleService.ChangeRoleAsync`, bypassing the aggregate by design). `StoredEventDeserializer.Deserialize` required its result to be `IDomainEvent`, so `PostgresUserRepository.GetByIdAsync` crashed with `InvalidOperationException` the moment it tried to rehydrate a user whose stream contained a role-change event — live in production, since `Program.cs` wires `PostgresUserRepository` and `UserRoleService` to the same `PostgresEventStore` singleton. `ProjectionRunner` had the identical crash on replay, for the same reason, for any projection handling `UserAppRoleChangedEvent` (`UserSummaryProjection` does).

## Decision

`StoredEventDeserializer.Deserialize` returns `IEvent` (the common base of `IDomainEvent` and `IApplicationEvent`) instead of requiring `IDomainEvent` — this is what fixes `ProjectionRunner`, which legitimately needs to hand both kinds to whichever projection declares a handler for them.

`AggregateRoot`/`User`, however, stay strictly domain-typed: `LoadFromHistory`/`Rehydrate` still only accept `IEnumerable<IDomainEvent>`. `User` never needs to know an `IApplicationEvent` exists — a role change isn't a domain concept, and widening the aggregate's replay contract to "any event, I'll ignore what I don't recognize" would blur that boundary for no real benefit. What changed instead: both methods now also take an explicit `streamVersion` parameter, supplied by the caller, rather than deriving `Version` by counting the domain events it was just given. `PostgresUserRepository.GetByIdAsync` filters the raw stream down to `IDomainEvent`s for `Rehydrate`'s event sequence, but separately computes `stored.Max(e => e.StreamVersion)` — the real physical stream position, application events included — and passes that in directly.

## Considered options

- **Widen `Rehydrate`/`LoadFromHistory` to `IEnumerable<IEvent>`, skip `Apply` for non-domain events, still derive `Version` by counting every item** — tried first, rejected. It fixed the crash, but conflated two different pieces of information (which events to replay vs. what stream position this is) into one parameter, and leaked an infrastructure concern (Postgres's append-only stream position) into the aggregate's public contract. It also turned out to be *wrong* on its own: an even earlier attempt that filtered application events out before `Rehydrate` compiled and looked plausible, but silently broke optimistic concurrency, since `Version` then undercounted relative to the event store's real `StreamVersion` and every subsequent `SaveAsync` for that user threw a false `ConcurrencyConflictException`. Making the version an explicit parameter, supplied by whoever actually reads the raw stream, removes that whole class of mistake instead of relying on every future caller to remember to count correctly.

## Consequences

Any repository whose aggregate shares a stream with `IApplicationEvent`s must compute and pass the real stream version explicitly — it's no longer inferred automatically. `InMemUserRepository` is unaffected (its stream is always domain-events-only, so `streamVersion: events.Count` is correct there). `User.Apply`'s `default` case still throws for a genuinely unrecognized `IDomainEvent`, unchanged and correct.
