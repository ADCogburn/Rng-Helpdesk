import { QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider, useLocation, type RouteObject } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ToastProvider } from '@/components/ui'
import { createQueryClient } from '@/lib/queryClient'
import { AuthLayout, RequireAdmin, RequireAuth } from '.'

const user = (appRole: string) => ({
  id: '1',
  appRole,
  clanPoints: 0,
  rank: 'Bronze',
  isActive: true,
  dateCreated: '2026-01-01T00:00:00Z',
  discordAccount: { discordId: '1', username: 'x' },
  runescapeAccounts: [],
})

function Where() {
  const { pathname, search } = useLocation()
  return <p data-testid="where">{pathname + search}</p>
}
const page = (name: string): RouteObject => ({
  element: (
    <>
      <h1>{name}</h1>
      <Where />
    </>
  ),
})

const routes: RouteObject[] = [
  {
    element: <AuthLayout />,
    children: [
      { path: 'login', ...page('Login') },
      {
        element: <RequireAuth allowPasswordChange />,
        children: [{ path: 'account/change-password', ...page('Pw') }],
      },
      {
        element: <RequireAuth />,
        children: [
          { path: 'me', ...page('Me') },
          { element: <RequireAdmin />, children: [{ path: 'admin', ...page('Admin') }] },
        ],
      },
    ],
  },
]

function renderAt(path: string) {
  const router = createMemoryRouter(routes, { initialEntries: [path] })
  render(
    <QueryClientProvider client={createQueryClient()}>
      <ToastProvider>
        <RouterProvider router={router} />
      </ToastProvider>
    </QueryClientProvider>,
  )
  return router
}

function mockMe(role: string) {
  vi.stubGlobal(
    'fetch',
    vi.fn(async () => new Response(JSON.stringify(user(role)), { status: 200 })),
  )
}

describe('route guards', () => {
  beforeEach(() => localStorage.clear())
  afterEach(() => {
    localStorage.clear()
    vi.unstubAllGlobals()
  })

  it('sends anonymous visitors to /login with ?next=', async () => {
    renderAt('/admin')
    expect(await screen.findByRole('heading', { name: 'Login' })).toBeInTheDocument()
    expect(screen.getByTestId('where')).toHaveTextContent('/login?next=%2Fadmin')
  })

  it('lets a member reach /me but bounces them from /admin to /me', async () => {
    localStorage.setItem('rng-token', 't')
    mockMe('Member')
    renderAt('/admin')
    expect(await screen.findByRole('heading', { name: 'Me' })).toBeInTheDocument()
  })

  it.each(['Administrator', 'SuperAdministrator', 'Owner'])('lets %s into /admin', async (role) => {
    localStorage.setItem('rng-token', 't')
    mockMe(role)
    renderAt('/admin')
    expect(await screen.findByRole('heading', { name: 'Admin' })).toBeInTheDocument()
  })

  it('holds users who must change their password on the change-password route', async () => {
    localStorage.setItem('rng-token', 't')
    localStorage.setItem('rng-must-change-password', '1')
    mockMe('Owner')
    renderAt('/admin')
    expect(await screen.findByRole('heading', { name: 'Pw' })).toBeInTheDocument()
    expect(screen.getByTestId('where')).toHaveTextContent('/account/change-password?next=%2Fadmin')
  })

  it('shows a retryable error when /auth/me cannot be reached', async () => {
    localStorage.setItem('rng-token', 't')
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')))
    renderAt('/me')
    expect(await screen.findByText('Could not load your account')).toBeInTheDocument()
    mockMe('Member')
    await userEvent.click(screen.getByRole('button', { name: /try again/i }))
    expect(await screen.findByRole('heading', { name: 'Me' })).toBeInTheDocument()
  })

  it('navigates to /login in-app and clears the session when a request returns 401', async () => {
    localStorage.setItem('rng-token', 't')
    vi.stubGlobal(
      'fetch',
      vi.fn(async () => new Response(null, { status: 401 })),
    )
    const router = renderAt('/me')
    await waitFor(() => expect(router.state.location.pathname).toBe('/login'))
    expect(router.state.location.search).toBe('?next=%2Fme')
    expect(localStorage.getItem('rng-token')).toBeNull()
  })
})
