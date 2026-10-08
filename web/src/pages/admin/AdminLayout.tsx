import { Outlet } from 'react-router'

/** Admin app shell (sidebar + top bar). F4 fills this in; F3 adds the RequireAdmin guard. */
export function Component() {
  return (
    <div className="min-h-screen">
      <Outlet />
    </div>
  )
}
