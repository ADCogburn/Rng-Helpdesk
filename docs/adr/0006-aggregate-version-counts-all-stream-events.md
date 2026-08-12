---
status: accepted
---

# Aggregate version counts every stream event, not just applied ones

Found while building the `ChangeUserRoleHandler` Postgres integration test from [ADR 0004](0004-two-tier-operations-handler-testing.md): `UserAppRoleChangedEvent` (an `IApplicationEvent`) is appended to the same `"User"` stream `PostgresUserRepository` uses for `User`'s own `IDomainEvent`s (`UserRoleService.ChangeRoleAsync`, bypassing the aggregate by design). `StoredEventDeserializer.Deserialize` required its result to be `IDomainEvent`, so `PostgresUserRepository.GetByIdAsync` crashed with `InvalidOperationException` the moment it tried to rehydrate a user whose stream contained a role-change event — live in production, since `Program.cs` wires `PostgresUserRepository` and `UserRoleService` to the same `PostgresEventStore` singleton. `ProjectionRunner` had the identical crash on replay, for the same reason, for any projection handling `UserAppRoleChangedEvent` (`UserSummaryProjection` does).

## Decision

`StoredEventDeserializer.Deserialize` now returns `IEvent` (the common base of `IDomainEvent` and `IApplicationEvent`) instead of requiring `IDomainEvent`. `AggregateRoot.LoadFromHistory`/`User.Rehydrate` now accept `IEnumerable<IEvent>`: only events that are `IDomainEvent` get `Apply`'d to aggregate state, but `Version` advances for *every* event in the sequence, application events included.

This isn't just "don't crash" — an earlier fix that filtered application events out before calling `Rehydrate` would have compiled and passed a naive test, but it silently broke optimistic concurrency instead: `Version` would then only count applied domain events, permanently undercounting relative to the event store's real `StreamVersion` for any user whose stream contains an application event, so every subsequent `SaveAsync` for that user would fail with a false `ConcurrencyConflictException`. `Version` has to track the same thing `PostgresEventStore.AppendAsync`'s unique-constraint check tracks — every append to the stream — not just the subset the aggregate cares about applying.

## Consequences

Any future `IApplicationEvent` sharing an aggregate's stream gets this behavior for free — it's a property of `AggregateRoot`, not something each aggregate has to handle itself. `User.Apply`'s `default` case still throws for a genuinely unrecognized `IDomainEvent`, which is unchanged and correct; only non-domain events are now silently skipped during `Apply` (and still counted).
