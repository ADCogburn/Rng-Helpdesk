---
status: accepted
---

# Two-tier Operations handler testing

`RngHelpdesk.Operations.Tests` exercises every command/query handler against `OperationsTestFixture`,
which wires real (but non-persistent) fakes — `InMemUserRepository`, `InMemoryEventStore`,
`InMemoryCredentialStore`, `InMemoryRankThresholdProvider` — including the same projection
singleton-sharing `Program.cs` uses in production. We're adding a second tier: handler-level
integration tests in `RngHelpdesk.Infrastructure.Tests` that wire the same handlers against the
real Postgres-backed repositories/event store via Testcontainers. Both tiers stay; neither replaces
the other.

## Why both

The fakes are real implementations of the repository/event-store/dispatcher interfaces, not mocks —
a fixture-backed test genuinely proves a handler's orchestration is correct (aggregate behavior →
`SaveAsync` → `Dispatch` → every projection updates), fast and without a container. What it
structurally can't catch is a persistence-layer bug: the fakes never serialize anything, so an event
type that's broken for JSON round-tripping (e.g. missing `[JsonConstructor]`) would pass every
fixture-backed test and only fail once real Postgres round-trips it through `PostgresEventStore`.
The new Postgres-backed handler tests exist to catch exactly that class of bug; they don't need to
re-assert handler business logic the fixture tests already cover.

## Consequences

Every handler now has tests in two projects that can look redundant at a glance — 20 handlers each
covered by both an `OperationsTestFixture` test and a Postgres-backed integration test. That's
intentional: the two check different things (orchestration logic vs. real persistence fidelity), not
the same thing twice. Don't delete either tier to "de-duplicate" without addressing what the other
tier stops catching.
