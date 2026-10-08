import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { useLocation, useNavigate } from 'react-router'
import { ApiError } from '@/api/errors'
import { fetchMe } from '@/api/hooks/auth'
import { queryKeys } from '@/api/queryKeys'
import { clearSession, getToken, setToken, setUnauthorizedHandler } from '@/api/session'
import type { GetUserResponse } from '@/api/types'
import { AuthContext, type AuthContextValue, type AuthStatus } from './AuthContext'

const MUST_CHANGE_KEY = 'rng-must-change-password'

function readMustChange(): boolean {
  try {
    return localStorage.getItem(MUST_CHANGE_KEY) === '1'
  } catch {
    return false
  }
}

function writeMustChange(value: boolean): void {
  try {
    if (value) localStorage.setItem(MUST_CHANGE_KEY, '1')
    else localStorage.removeItem(MUST_CHANGE_KEY)
  } catch {
    /* storage unavailable */
  }
}

/** Owns the session: token in localStorage, `/auth/me` on boot, in-app 401 handling. Must sit inside the router. */
export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const location = useLocation()
  const [token, setTokenState] = useState<string | null>(() => getToken())
  const [mustChangePassword, setMustChange] = useState<boolean>(() => readMustChange())

  // The 401 handler is registered once but needs the live location.
  const locationRef = useRef(location)
  useEffect(() => {
    locationRef.current = location
  }, [location])

  const me = useQuery({
    queryKey: queryKeys.auth.me,
    queryFn: fetchMe,
    enabled: !!token,
    retry: false,
    staleTime: 60_000,
  })

  const reset = useCallback(() => {
    clearSession()
    writeMustChange(false)
    setTokenState(null)
    setMustChange(false)
    queryClient.clear()
  }, [queryClient])

  useEffect(() => {
    setUnauthorizedHandler(() => {
      reset()
      const { pathname, search } = locationRef.current
      if (pathname.startsWith('/login')) return
      navigate(`/login?next=${encodeURIComponent(pathname + search)}`, { replace: true })
    })
    return () => setUnauthorizedHandler(null)
  }, [navigate, reset])

  const signIn = useCallback<AuthContextValue['signIn']>(
    async (newToken, mustChange) => {
      queryClient.clear()
      setToken(newToken)
      let user: GetUserResponse
      try {
        user = await queryClient.fetchQuery({
          queryKey: queryKeys.auth.me,
          queryFn: fetchMe,
          staleTime: 0,
        })
      } catch (error) {
        clearSession()
        throw error
      }
      writeMustChange(mustChange)
      setMustChange(mustChange)
      setTokenState(newToken)
      return user
    },
    [queryClient],
  )

  const passwordChanged = useCallback(() => {
    writeMustChange(false)
    setMustChange(false)
  }, [])

  const { refetch } = me
  const retry = useCallback(() => void refetch(), [refetch])

  const value = useMemo<AuthContextValue>(() => {
    let status: AuthStatus
    if (!token) status = 'anonymous'
    else if (me.data) status = 'authenticated'
    else if (me.isError && !(me.error instanceof ApiError && me.error.status === 401))
      status = 'error'
    else status = 'loading'
    return {
      status,
      user: me.data ?? null,
      mustChangePassword,
      error: me.error,
      signIn,
      signOut: reset,
      passwordChanged,
      retry,
    }
  }, [
    token,
    me.data,
    me.isError,
    me.error,
    mustChangePassword,
    signIn,
    reset,
    passwordChanged,
    retry,
  ])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
