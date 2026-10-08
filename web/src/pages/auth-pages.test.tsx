import { QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider, useLocation } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthLayout } from '@/auth'
import { ToastProvider } from '@/components/ui'
import { createQueryClient } from '@/lib/queryClient'
import { validatePasswordChange } from '@/auth/passwordRules'
import { Component as ChangePasswordPage } from './ChangePasswordPage'
import { Component as LoginPage } from './LoginPage'

const me = (appRole: string) => ({
  id: '1',
  appRole,
  clanPoints: 0,
  rank: 'Bronze',
  isActive: true,
  dateCreated: '2026-01-01T00:00:00Z',
  discordAccount: { discordId: '1', username: 'x' },
  runescapeAccounts: [],
})

const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status })

function Where() {
  const { pathname, search } = useLocation()
  return <p data-testid="where">{pathname + search}</p>
}

function renderAt(path: string) {
  const router = createMemoryRouter(
    [
      {
        element: <AuthLayout />,
        children: [
          { path: 'login', element: <LoginPage /> },
          { path: 'account/change-password', element: <ChangePasswordPage /> },
          { path: '*', element: <Where /> },
        ],
      },
    ],
    { initialEntries: [path] },
  )
  render(
    <QueryClientProvider client={createQueryClient()}>
      <ToastProvider>
        <RouterProvider router={router} />
      </ToastProvider>
    </QueryClientProvider>,
  )
  return router
}

async function signIn() {
  await userEvent.type(screen.getByLabelText(/username/i), 'admin')
  await userEvent.type(screen.getByLabelText(/password/i), 'password')
  await userEvent.click(screen.getByRole('button', { name: 'Sign in' }))
}

beforeEach(() => localStorage.clear())
afterEach(() => {
  localStorage.clear()
  vi.unstubAllGlobals()
})

describe('LoginPage', () => {
  function stubApi(login: () => Response | Promise<Response>, role = 'Owner') {
    vi.stubGlobal(
      'fetch',
      vi.fn(async (url: string) => (url.endsWith('/auth/login') ? login() : json(me(role)))),
    )
  }

  it('disables submit until both fields are filled', async () => {
    renderAt('/login')
    expect(screen.getByRole('button', { name: 'Sign in' })).toBeDisabled()
  })

  it('sends admins to /admin and members to /me', async () => {
    stubApi(() => json({ token: 'jwt', mustChangePassword: false }), 'Owner')
    const router = renderAt('/login')
    await signIn()
    await waitFor(() => expect(router.state.location.pathname).toBe('/admin'))
    expect(localStorage.getItem('rng-token')).toBe('jwt')
  })

  it('sends members to /me', async () => {
    stubApi(() => json({ token: 'jwt', mustChangePassword: false }), 'Member')
    const router = renderAt('/login')
    await signIn()
    await waitFor(() => expect(router.state.location.pathname).toBe('/me'))
  })

  it('honours a safe ?next= and ignores an external one', async () => {
    stubApi(() => json({ token: 'jwt' }))
    const router = renderAt('/login?next=%2Fadmin%2Franks')
    await signIn()
    await waitFor(() => expect(router.state.location.pathname).toBe('/admin/ranks'))
  })

  it('ignores an open-redirect ?next=', async () => {
    stubApi(() => json({ token: 'jwt' }))
    const router = renderAt('/login?next=%2F%2Fevil.example')
    await signIn()
    await waitFor(() => expect(router.state.location.pathname).toBe('/admin'))
  })

  it('goes to change-password when mustChangePassword is true', async () => {
    stubApi(() => json({ token: 'jwt', mustChangePassword: true }))
    const router = renderAt('/login?next=%2Fme')
    await signIn()
    await waitFor(() => expect(router.state.location.pathname).toBe('/account/change-password'))
    expect(router.state.location.search).toBe('?next=%2Fme')
  })

  it('shows a bad-credentials error', async () => {
    stubApi(() => new Response('Invalid credentials', { status: 401 }))
    renderAt('/login')
    await signIn()
    expect(await screen.findByRole('alert')).toHaveTextContent('Incorrect username or password.')
    expect(localStorage.getItem('rng-token')).toBeNull()
  })

  it('shows a network error', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')))
    renderAt('/login')
    await signIn()
    expect(await screen.findByRole('alert')).toHaveTextContent('Could not reach the server')
  })

  it('shows a server error', async () => {
    stubApi(() => new Response(null, { status: 500 }))
    renderAt('/login')
    await signIn()
    expect(await screen.findByRole('alert')).toHaveTextContent('server ran into a problem')
  })
})

describe('validatePasswordChange', () => {
  it('accepts a valid change', () => {
    expect(validatePasswordChange('oldpassword', 'newpassword', 'newpassword')).toEqual({})
  })
  it('rejects short, identical and mismatched passwords', () => {
    expect(validatePasswordChange('old', 'short', 'short').newPassword).toMatch(/at least 8/)
    expect(
      validatePasswordChange('samepassword', 'samepassword', 'samepassword').newPassword,
    ).toMatch(/differ/)
    expect(validatePasswordChange('old', 'newpassword', 'other').confirmPassword).toMatch(/match/)
    expect(validatePasswordChange('', 'newpassword', 'newpassword').currentPassword).toBeDefined()
  })
})

describe('ChangePasswordPage', () => {
  beforeEach(() => {
    localStorage.setItem('rng-token', 't')
    localStorage.setItem('rng-must-change-password', '1')
  })

  async function fill(current: string, next: string, confirm: string) {
    await screen.findByRole('heading', { name: 'Change password' })
    await userEvent.type(screen.getByLabelText(/^Current password/), current)
    await userEvent.type(screen.getByLabelText(/^New password/), next)
    await userEvent.type(screen.getByLabelText(/^Confirm new password/), confirm)
    await userEvent.click(screen.getByRole('button', { name: 'Change password' }))
  }

  it('shows validation errors without calling the API', async () => {
    const fetchMock = vi.fn(async () => json(me('Owner')))
    vi.stubGlobal('fetch', fetchMock)
    renderAt('/account/change-password')
    await fill('oldpassword', 'short', 'different')
    expect(await screen.findByText('Use at least 8 characters.')).toBeInTheDocument()
    expect(screen.getByText('Passwords do not match.')).toBeInTheDocument()
    expect(
      (fetchMock.mock.calls as unknown[][]).some(([u]) => String(u).endsWith('/change-password')),
    ).toBe(false)
  })

  it('changes the password, clears the forced flag and moves on', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async (url: string) =>
        url.endsWith('/auth/change-password')
          ? new Response(null, { status: 204 })
          : json(me('Owner')),
      ),
    )
    const router = renderAt('/account/change-password?next=%2Fadmin%2Franks')
    await fill('oldpassword', 'newpassword1', 'newpassword1')
    await waitFor(() => expect(router.state.location.pathname).toBe('/admin/ranks'))
    expect(localStorage.getItem('rng-must-change-password')).toBeNull()
  })

  it('surfaces a server rejection and stays put', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async (url: string) =>
        url.endsWith('/auth/change-password')
          ? new Response('Current password is incorrect.', { status: 400 })
          : json(me('Owner')),
      ),
    )
    const router = renderAt('/account/change-password')
    await fill('wrongpassword', 'newpassword1', 'newpassword1')
    expect(await screen.findByRole('alert')).toHaveTextContent('Current password is incorrect.')
    expect(router.state.location.pathname).toBe('/account/change-password')
  })
})
