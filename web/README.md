# RngHelpdesk web

React 19 + Vite + TypeScript (strict) + Tailwind CSS v4 + React Router 7 (data mode) + TanStack Query 5.
Plan and design direction: [`../docs/ui/PLAN.md`](../docs/ui/PLAN.md).

## Commands

```powershell
npm install
npm run dev      # http://localhost:5173, proxies /api/* -> https://localhost:5081 (strips /api)
npm run build    # tsc -b && vite build
npm run lint
npm test         # vitest run (jsdom)
npm run format   # prettier --write .
```

The API must be running (`dotnet run --project RngHelpdesk.Api`, see root CLAUDE.md) for real data.
Dev login: `admin` / `password`.

## Layout

- `src/api/` - `client.ts` (fetch wrapper, base `/api`, bearer token, 401 handling), `errors.ts`
  (`ApiError {status, message, fieldErrors?}`), `session.ts` (token in localStorage,
  `setUnauthorizedHandler` for F3), `types.ts` (contract types; ids are strings), `queryKeys.ts`,
  `hooks/{auth,public,users,admin}.ts` (TanStack Query hooks; mutations invalidate related keys).
  Import everything from `@/api`.
- `src/components/ui/` - primitives, import from `@/components/ui`: Button (+`buttonClass()` for
  link-styled buttons), Card/CardHeader, FormField (label + control + error wiring), Input, Select,
  Textarea, Label, Badge, RankBadge, Table (TableHead/TableBody/Tr/Th/Td, sortable `Th`), Dialog,
  ConfirmDialog, Tabs/TabPanel, ToastProvider/`useToast()`, Spinner, Skeleton, EmptyState,
  ErrorState, StatCard, PageHeader, ThemeToggle.
- `src/lib/` - `ranks.ts` (colours, labels, role helpers), `format.ts`, `theme.ts`, `cn.ts`.
- `src/config/clan.ts` - clan name, tagline, Discord invite, about/join copy.
- `src/pages/` - lazy route modules exporting `Component`; most are placeholders to be filled by F2-F8.
- `src/router.tsx` - `routes` array (also used by tests via `createMemoryRouter`).
- `src/index.css` - theme tokens (`bg-bg`, `bg-surface`, `bg-raised`, `border-line`, `text-fg`,
  `text-muted`, `bg-primary`, `text-danger`, `text-success`, `--color-rank-*`), dark default,
  light via `data-theme="light"` or OS preference.

Conventions: Tailwind utilities only; semantic color tokens, never hardcoded hex in components;
numbers use `tabular-nums`; keep component files exporting only components (fast refresh lint rule).
