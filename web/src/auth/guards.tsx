import { Navigate, Outlet, useLocation } from 'react-router'
import { RouteFallback } from '@/components/layout/RouteFallback'
import { ErrorState } from '@/components/ui'
import { homePathFor, isAdminPlus, useAuth } from './AuthContext'

export const CHANGE_PASSWORD_PATH = '/account/change-password'

function loginPath(pathname: string, search: string) {
  return `/login?next=${encodeURIComponent(pathname + search)}`
}

/**
 * Layout guard for signed-in routes. Anonymous visitors go to `/login?next=`; users who still owe
 * a password change are held on the change-password page (which opts out via `allowPasswordChange`).
 */
export function RequireAuth({ allowPasswordChange = false }: { allowPasswordChange?: boolean }) {
  const { status, mustChangePassword, error, retry } = useAuth()
  const { pathname, search } = useLocation()

  if (status === 'loading') return <RouteFallback />
  if (status === 'anonymous') return <Navigate to={loginPath(pathname, search)} replace />
  if (status === 'error')
    return (
      <div className="mx-auto max-w-lg p-6">
        <ErrorState error={error} title="Could not load your account" onRetry={retry} />
      </div>
    )
  if (mustChangePassword && !allowPasswordChange) {
    const next = pathname + search
    return <Navigate to={`${CHANGE_PASSWORD_PATH}?next=${encodeURIComponent(next)}`} replace />
  }
  return <Outlet />
}

/** Nest inside `RequireAuth`. Non-admin members are bounced to `/me`. */
export function RequireAdmin() {
  const { user } = useAuth()
  if (!isAdminPlus(user?.appRole)) return <Navigate to={homePathFor(user?.appRole)} replace />
  return <Outlet />
}
