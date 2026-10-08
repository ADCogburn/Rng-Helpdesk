import { ApiError } from '@/api/errors'

export type LoginFailure = 'credentials' | 'network' | 'server'

export function classifyLoginError(error: unknown): LoginFailure {
  if (error instanceof ApiError) {
    if (error.status === 0) return 'network'
    if (error.status >= 500) return 'server'
    return 'credentials'
  }
  return 'network'
}

export const loginFailureMessage: Record<LoginFailure, string> = {
  credentials: 'Incorrect username or password.',
  network: 'Could not reach the server. Check your connection and try again.',
  server: 'The server ran into a problem. Please try again in a moment.',
}
