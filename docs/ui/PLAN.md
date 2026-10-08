# UI redo plan â€” clan landing page + admin console

Branch: `feature/ui-redo` (off `origin/development`, PR targets `development`).
Orchestrated from a Herdr pane; tasks below are executed by worker agents (Haiku/Sonnet) and
reviewed + committed by the orchestrator. **Workers never commit, push, or open PRs.**

## Goals

1. A beautiful, public **landing page** for the clan â€” live stats, the rank ladder, a leaderboard,
   and a "join us" call to action.
2. A proper **login** (JWT via `POST /auth/login`), and an **admin console** for `AdminPlus` roles
   that covers every admin capability the API exposes.
3. Close the API gaps that block (1) and (2).

Out of scope: Discord OAuth, password reset by email (#16), preferred-username pipeline (#13/#14),
live rank-resolver refresh after threshold edits (documented gap in CLAUDE.md â€” the UI just
warns that edits apply after an API restart).

## Decisions (made with the user, 2026-10-07)

- Stack: **React 19 + Vite + TypeScript (strict) + Tailwind CSS v4 + React Router + TanStack
  Query**, Vitest + Testing Library for tests. App lives in **`web/`** (old `RngHelpdesk.Website/`,
  `RngHelpdesk.Web/`, `web/` leftovers were deleted).
- Landing page data comes from **new anonymous `/public/*` endpoints** (stats, rank ladder,
  leaderboard). No Discord IDs or Discord usernames are exposed publicly â€” RSN, rank, points only.
- `ulong` IDs (Discord snowflakes) are **serialized as JSON strings** API-wide. They exceed
  `Number.MAX_SAFE_INTEGER`; as numbers they are silently corrupted by any JS client. The API must
  also accept them as strings (and numbers, for backwards compat) on input.
- Feature #76 (deactivate/reactivate) is merged into this branch.

## API contract (what the UI targets)

All JSON. Enums serialize as **strings** (`JsonStringEnumConverter`). IDs are strings after task B1.
Errors: `400` body is a plain string (handler error) or a ValidationProblemDetails (FluentValidation);
`404` plain string; `401` empty.

| Method | Route | Auth | Body â†’ Response |
|---|---|---|---|
| POST | `/auth/login` | anon | `{username,password}` â†’ `{token, mustChangePassword}` (flag added in B3) |
| GET | `/auth/me` | any JWT | â†’ `GetUserResponse` |
| POST | `/auth/change-password` | any JWT | `{currentPassword,newPassword}` â†’ 204 (added in B3) |
| GET | `/public/overview` | anon | â†’ `{activeMemberCount,totalClanPoints,rankDistribution:[{rank,count}]}` (B2) |
| GET | `/public/ranks` | anon | â†’ `{ranks:[{rank,pointsRequired}]}` ascending (B2) |
| GET | `/public/leaderboard?top=25` | anon | â†’ `{entries:[{position,runescapeUsername,rank,clanPoints}]}` (B2) |
| GET | `/users` | AdminPlus | â†’ `{totalCount, users: GetUserResponse[]}` |
| GET | `/users/{id}` | AdminPlus | â†’ `GetUserResponse` |
| GET | `/users/by-rsn/{rsn}` | AdminPlus | â†’ `GetUserResponse` |
| GET | `/users/by-historical-rsn/{rsn}` | AdminPlus | â†’ `{users: GetUserResponse[]}` |
| GET | `/users/{id}/lifecycle` | AdminPlus | â†’ `{userId, history:[{action,occurredAt}]}` |
| POST | `/users/{id}/points/add` | AdminPlus | `{points,reason}` â†’ 204 |
| POST | `/users/{id}/points/remove` | AdminPlus | `{points,reason}` â†’ 204 |
| GET | `/users/{id}/point-history` | AdminPlus | â†’ `{userId,totalEventCount,events:[{delta,reason,occurredAt,rankBefore?,rankAfter?}]}` |
| GET | `/users/{id}/runescape-accounts` | AdminPlus | â†’ `{accounts:[{username}]}` |
| GET | `/users/{id}/runescape-accounts/previous` | AdminPlus | â†’ `{accounts:[{username}]}` |
| GET | `/users/{id}/runescape-accounts/history` | AdminPlus | â†’ `{history:[{changeType,username?,oldUsername?,newUsername?,occurredAt}]}` |
| POST | `/users/{id}/runescape-accounts` | AdminPlus | `{username}` â†’ 200 |
| DELETE | `/users/{id}/runescape-accounts` | AdminPlus | `{username}` â†’ 200 |
| PUT | `/users/{id}/runescape-accounts/rename` | AdminPlus | `{oldUsername,newUsername}` â†’ 204 |
| POST | `/admin/create` | AdminPlus | `{discordAccount:{discordId,username}, runescapeAccounts:[{username}]}` â†’ 201 `{userId,username,temporaryPassword}` |
| POST | `/admin/{id}/promote` | AdminPlus | â†’ 204 |
| POST | `/admin/{id}/demote` | AdminPlus | â†’ 204 |
| POST | `/admin/{id}/deactivate` | AdminPlus | â†’ 204 (check AdminController for exact codes) |
| POST | `/admin/{id}/reactivate` | AdminPlus | â†’ 204 |
| GET | `/rankthresholds` | AdminPlus | â†’ `{thresholds:[{rank,pointsRequired}]}` |
| PUT | `/rankthresholds/{rank}` | AdminPlus | `{pointsRequired}` â†’ 204 (must stay strictly between neighbours) |

`GetUserResponse` = `{id, appRole, clanPoints, rank, isActive, dateCreated, discordAccount:{discordId,username}, runescapeAccounts:[{username}]}`.

Enums: `AppRole` = `Member|Administrator|SuperAdministrator|Owner` (AdminPlus = the last three).
`Rank` = `Bronze, Iron, Steel, Mithril, Adamant, Rune, Dragon, Sapphire, Emerald, Ruby, Diamond,
Dragonstone, Onyx, Zenyte` (point-based, ascending) then `Administrator, DeputyOwner, Owner`
(role-based overrides). RSN rule: `^(?! )[A-Za-z0-9 -]{1,12}(?<! )$`.

JWT claims: `ClaimTypes.NameIdentifier` (user id) and `ClaimTypes.Role` (AppRole name). The UI should
not decode the token for authorization decisions â€” call `/auth/me` and use `appRole`.

Dev: API at `https://localhost:5081` (VS Code `http` profile, see CLAUDE.md). Seeded dev login
`admin` / `password` (Owner). Vite dev server proxies `/api/*` â†’ `https://localhost:5081/*`
(strip `/api`, `secure: false`), so no CORS is needed in dev.

## Design direction â€” "Old School, polished"

Dark-first, RuneScape-flavoured but modern. Not a parody of the game UI.

- **Palette (CSS variables on `:root`, Tailwind v4 `@theme`)**: background obsidian `#0d0f14`,
  surface `#161a22`, raised `#1e2330`, border `#2a3040`; text parchment `#ece3cf`, muted `#9a9488`;
  primary gold `#d4a84b` (hover `#e8c26a`); danger `#d0573f`; success `#5fae6e`. Provide a light
  theme ("parchment": bg `#f5efe1`, text `#231f17`) via `data-theme="light"` + `prefers-color-scheme`.
- **Rank colours** (badges, ladder, charts) â€” one token per rank: Bronze `#a8703a`, Iron `#8a8d91`,
  Steel `#b4bcc6`, Mithril `#5a64b8`, Adamant `#4f8a5b`, Rune `#4fb3c7`, Dragon `#c0392b`,
  Sapphire `#2f6fd6`, Emerald `#2ea86b`, Ruby `#c21f4a`, Diamond `#cfe8f5`, Dragonstone `#a35bd6`,
  Onyx `#2b2b2b` (with light border), Zenyte `#f0a33a`; Administrator/DeputyOwner/Owner use gold
  with a crown/shield icon.
- **Type**: headings `Cinzel` (Google Fonts), body `Inter`. Numbers use `tabular-nums`.
- **Motion**: subtle only â€” hero ember/particle glow via CSS, hover lifts, `prefers-reduced-motion`
  respected.
- **Icons**: `lucide-react`. Charts: `recharts` (point history only).
- Responsive down to 360px; keyboard accessible; visible focus rings (gold).

Clan identity lives in **`web/src/config/clan.ts`** (name `"RNG"`, tagline, Discord invite URL,
about copy, values/feature bullets). Placeholders are fine; the user fills them in.

## Information architecture

Public:
- `/` â€” Landing: sticky nav (logo, Ranks, Leaderboard, Join, "Sign in"), hero (clan name, tagline,
  Join Discord CTA, live stat chips), "About the clan" feature cards, **Rank ladder** (14 tiers,
  coloured, points required, member count per tier from `rankDistribution`), **Leaderboard** (top 25,
  podium styling for top 3), "How to join" steps, footer.
- `/login` â€” centered card, error states, redirects to `?next=` or role home.

Authenticated:
- `/account/change-password` â€” forced after login when `mustChangePassword` is true.
- `/me` â€” non-admin members: own profile (rank, points, progress to next rank, RSNs) from `/auth/me`.
- `/admin` (AdminPlus only; others â†’ `/me`) â€” app shell: sidebar (Dashboard, Members, Add member,
  Rank thresholds), top bar (current user, role badge, theme toggle, sign out).
  - `/admin` Dashboard: stat cards, rank distribution bar, newest members, quick search by RSN.
  - `/admin/members`: searchable/sortable table (RSN, Discord name, rank, points, role, status),
    filters (rank, role, active/inactive), search also tries historical RSN on enter.
  - `/admin/members/:id`: profile header + tabs â€” **Overview** (points adjust form, role
    promote/demote, deactivate/reactivate with confirm dialogs), **RuneScape accounts**
    (link/delink/rename, previous RSNs, history timeline), **Points** (history table + cumulative
    line chart), **Lifecycle** (timeline).
  - `/admin/members/new`: create form (Discord ID + username, 0..n RSNs) â†’ success panel showing
    generated username + temporary password once, with copy buttons and a warning.
  - `/admin/ranks`: editable threshold table with inline monotonic validation and a note that
    changes apply to rank resolution after an API restart.

Guarding a self-destructive action: block promote/demote/deactivate on your own user id in the UI.

## Tasks

Model: **H** = Haiku, **S** = Sonnet. "Done when" = acceptance criteria the orchestrator checks.
Every backend task: `dotnet build RngHelpdesk.slnx` clean + `dotnet test` for the touched test
projects green. Every frontend task: `npm run build`, `npm run lint`, `npm test` green in `web/`.

### Backend lane (serial â€” they share `Program.cs` and test fixtures)

- **B1 (S) â€” ulong-as-string wire format.** Add a `JsonConverter<ulong>` (write string; read string
  or number) registered in `Program.cs` JSON options. Fix any Api.Tests that assert on numeric ids in
  serialized JSON, and update Bruno requests whose JSON bodies send ids as numbers. Add tests for the
  converter. Done when: `GET /users` returns `"id":"123456789012345678"`.
- **B2 (S) â€” public endpoints.** Contracts (`Contracts/Public/...` queries + responses matching the
  table above), Operations query handlers reading `IUserSummaryReadStore` + `IRankThresholdProvider`,
  `PublicController` with `[AllowAnonymous]`, route `public`. Leaderboard: active users with â‰¥1 RSN,
  ordered by points desc then RSN, first RSN shown, `top` clamped 1..100 default 25. Overview counts
  active users only; distribution covers every point-based rank (zero counts included). Tests in
  Operations.Tests and Api.Tests following existing patterns (ADR-0004: add Postgres-tier tests only
  if the existing query handlers have them).
- **B3 (S) â€” auth gaps (#20 subset).** `LoginResponse.MustChangePassword`; `POST /auth/change-password`
  (`[Authorize]`, verifies current password via `ValidateCredentialsAsync` with the user's username,
  then `ChangePasswordAsync`), FluentValidation validator (new password â‰¥ 8 chars, differs from
  current). Tests.
- **B4 (H) â€” CORS config.** Replace hardcoded `http://localhost:55751` with `Cors:AllowedOrigins`
  string array from config (default in `appsettings.Development.json`: `http://localhost:5173`).
- **B5 (H) â€” Bruno.** Add `Public/*`, `Auth/Change Password`, `Admin/Deactivate User`,
  `Admin/Reactivate User` requests following existing collection conventions.

### Frontend lane

- **F1 (S) â€” scaffold + foundations.** Vite React-TS in `web/`, Tailwind v4, React Router, TanStack
  Query, lucide-react, ESLint, Prettier, Vitest + Testing Library + jsdom; Vite proxy as above;
  `src/api/` typed client (`fetch` wrapper: base `/api`, bearer token, JSON, error normalization into
  `ApiError {status, message, fieldErrors?}`, 401 â†’ clear session + redirect to `/login`);
  `src/api/types.ts` mirroring every contract above (ids as `string`); query-key + hook modules per
  resource; theme tokens + rank colour map; fonts; UI primitives in `src/components/ui/` (Button,
  Card, Input, Label, Select, Textarea, Badge, RankBadge, Table, Dialog/ConfirmDialog, Tabs, Toast,
  Spinner, Skeleton, EmptyState, ErrorState); `src/config/clan.ts`; router skeleton with lazy routes
  for every page in the IA (placeholder pages) so later tasks only fill page files; `web/README.md`;
  `.gitignore` entries. Done when: dev server renders placeholder routes, primitives have a smoke test.
- **F2 (S) â€” landing page.** Everything under "Public `/`" above, using `/public/*` hooks with
  skeleton/error/empty states. Must look great at 360px, 768px, 1440px.
- **F3 (S) â€” auth.** Login page, `AuthProvider` (token in `localStorage`, `/auth/me` on boot),
  `RequireAuth` / `RequireAdmin` guards, role-based landing after login, forced change-password page,
  sign-out. Tests for guards and login error states.
- **F4 (S) â€” admin shell + dashboard + members list.** Depends F3.
- **F5 (S) â€” member detail page** (all four tabs and every mutation, with optimistic invalidation and
  toasts; confirm dialogs for destructive actions; self-action guard). Depends F4.
- **F6 (H) â€” create member page.** Depends F4. Can run parallel with F5/F7.
- **F7 (H) â€” rank thresholds editor.** Depends F4. Can run parallel with F5/F6.
- **F8 (H) â€” `/me` member profile** with progress-to-next-rank bar. Depends F3.

### Wrap-up

- **D1 (S) â€” docs.** ADR-0007 (ulong ids as JSON strings + anonymous `/public` read endpoints),
  CLAUDE.md updates (Frontend status, Commands for `web/`, Solution layout, public endpoints, B4
  CORS config), `docs/ui/PLAN.md` status column.
- **Q1 (orchestrator) â€” end-to-end check** in a real browser against the running API + local
  Postgres; fix-up tasks dispatched to workers as needed; then PR to `development`.

## Status

| Task | Model | State |
|---|---|---|
| B1 | S | done |
| B2 | S | done |
| B3 | S | done |
| B4 | H | done |
| B5 | H | done |
| F1 | S | done |
| F2 | S | done |
| F3 | S | done |
| F4 | S | done |
| F5 | S | todo |
| F6 | H | todo |
| F7 | H | todo |
| F8 | H | done |
| D1 | S | done |
| Q1 | â€” | todo |
