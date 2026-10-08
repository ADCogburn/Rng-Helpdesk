import { createContext, useContext } from 'react'
import type { AppRole, GetUserResponse } from '@/api/types'

export type AuthStatus = 'loading' | 'authenticated' | 'anonymous' | 'error'

export interface AuthContextValue {
  status: AuthStatus
  user: GetUserResponse | null
  /** True until the user has replaced a temporary/forced password. */
  mustChangePassword: boolean
  /** Why `/auth/me` failed when status is `error` (network/server, not 401). */
  error: unknown
  /** Stores the token, loads `/auth/me` and returns the user. Throws (and clears the token) on failure. */
  signIn: (token: string, mustChangePassword: boolean) => Promise<GetUserResponse>
  /** Clears the token and the whole query cache. */
  signOut: () => void
  /** Call after a successful password change. */
  passwordChanged: () => void
  /** Re-attempts `/auth/me` after an `error` status. */
  retry: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within <AuthProvider>')
  return ctx
}

const ADMIN_ROLES: readonly AppRole[] = ['Administrator', 'SuperAdministrator', 'Owner']

/** AdminPlus on the API. */
export function isAdminPlus(role: AppRole | undefined | null): boolean {
  return !!role && ADMIN_ROLES.includes(role)
}

export function homePathFor(role: AppRole | undefined | null): string {
  return isAdminPlus(role) ? '/admin' : '/me'
}

/** Only same-origin absolute paths are honoured, so `?next=` cannot become an open redirect. */
export function safeNext(next: string | null | undefined): string | null {
  if (!next || !next.startsWith('/') || next.startsWith('//') || next.startsWith('/\\')) return null
  if (next.startsWith('/login')) return null
  return next
}
