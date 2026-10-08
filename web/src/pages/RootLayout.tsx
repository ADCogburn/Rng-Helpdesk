import { Outlet, ScrollRestoration } from 'react-router'

/** Top-level layout for every route. Global providers live in main.tsx. */
export function Component() {
  return (
    <>
      <Outlet />
      <ScrollRestoration />
    </>
  )
}
