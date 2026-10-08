import { Outlet } from 'react-router'
import { AuthProvider } from './AuthProvider'

/** Route-level wrapper: AuthProvider needs router hooks, so it lives inside the router tree. */
export function Component() {
  return (
    <AuthProvider>
      <Outlet />
    </AuthProvider>
  )
}
