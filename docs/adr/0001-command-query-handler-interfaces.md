---
status: accepted
---

# Command/query handler interfaces

Operations handlers were plain concrete classes with no abstraction behind them, which made it impossible to isolate a handler in an Api-layer controller test (no interface to mock). We introduced `ICommandHandler<TRequest>`, `ICommandHandler<TRequest,TResult>`, and `IQueryHandler<TRequest,TResponse>` — split by CQRS shape, matching the existing `Commands`/`Queries` split in `RngHelpdesk.Contracts` and the existing `CommandResult`/`QueryResult<T>` distinction — rather than a single MediatR-style generic interface, so the split is enforced at compile time rather than by convention.

## Considered options

- **Single generic `IHandler<TRequest,TResponse>`** (MediatR-shaped) — rejected in favor of the explicit split, even though it would make a future MediatR migration a more mechanical rename, because it would let a query handler return a `CommandResult` without the compiler objecting.
- **Sync `Handle` methods**, matching most handler bodies as they exist today — rejected. `Task<TResult> Handle(TRequest request, CancellationToken cancellationToken = default)` was chosen on both interfaces so that issue #8 ("Port to Async") doesn't force a second interface-breaking signature change later; sync handlers just wrap their body in `Task.FromResult(...)` for now.
- **Tolerating the existing result-type/parameter-shape mismatches** — `GetPointHistoryForUserHandler` and `GetUserLifecycleHistoryHandler` returned `CommandResult<T>` despite being query-shaped, and `GetAllUsersHandler`/`GetPreviousRunescapeAccountHandler`/`GetRunescapeAccountHistoryHandler` took no request object or a raw `ulong` instead of a request record — rejected. These 5 handlers were normalized (to `QueryResult<T>` and proper request records respectively) as a prerequisite, so every handler actually implements the interface it's given rather than baking the inconsistency into a brand-new abstraction.

## Consequences

- Every live Operations handler and its Api controller call site changes signature (adds `CancellationToken`, wraps currently-sync bodies in `Task.FromResult`).
- `DeactivateUserHandler` and `ReactivateUserHandler` remain empty/unimplemented and are excluded from this change until a separate issue fills them in.
