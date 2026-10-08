const TOKEN_KEY = 'rng-token'

export function getToken(): string | null {
  try {
    return localStorage.getItem(TOKEN_KEY)
  } catch {
    return null
  }
}

export function setToken(token: string): void {
  try {
    localStorage.setItem(TOKEN_KEY, token)
  } catch {
    /* storage unavailable */
  }
}

export function clearSession(): void {
  try {
    localStorage.removeItem(TOKEN_KEY)
  } catch {
    /* storage unavailable */
  }
}

type UnauthorizedHandler = () => void

function defaultUnauthorizedHandler() {
  if (window.location.pathname.startsWith('/login')) return
  const next = encodeURIComponent(window.location.pathname + window.location.search)
  window.location.assign(`/login?next=${next}`)
}

let unauthorizedHandler: UnauthorizedHandler = defaultUnauthorizedHandler

/** Lets the AuthProvider (F3) take over the 401 redirect (e.g. to navigate without a reload). */
export function setUnauthorizedHandler(handler: UnauthorizedHandler | null): void {
  unauthorizedHandler = handler ?? defaultUnauthorizedHandler
}

export function handleUnauthorized(): void {
  clearSession()
  unauthorizedHandler()
}
