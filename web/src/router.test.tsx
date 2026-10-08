import { QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ToastProvider } from '@/components/ui'
import { createQueryClient } from '@/lib/queryClient'
import { routes } from './router'

const cases: [string, string][] = [
  ['/login', 'Sign in'],
  ['/account/change-password', 'Change password'],
  ['/me', 'My profile'],
  ['/admin', 'Dashboard'],
  ['/admin/members', 'Members'],
  ['/admin/members/new', 'Add member'],
  ['/admin/members/123456789012345678', 'Member detail'],
  ['/admin/ranks', 'Rank thresholds'],
  ['/nope', 'Page not found'],
]

const owner = {
  id: '1',
  appRole: 'Owner',
  clanPoints: 0,
  rank: 'Owner',
  isActive: true,
  dateCreated: '2026-01-01T00:00:00Z',
  discordAccount: { discordId: '1', username: 'admin' },
  runescapeAccounts: [],
}

function Providers({ router }: { router: ReturnType<typeof createMemoryRouter> }) {
  return (
    <QueryClientProvider client={createQueryClient()}>
      <ToastProvider>
        <RouterProvider router={router} />
      </ToastProvider>
    </QueryClientProvider>
  )
}

describe('router skeleton', () => {
  it('/ renders the landing page', async () => {
    const router = createMemoryRouter(routes, { initialEntries: ['/'] })
    render(<Providers router={router} />)
    expect(await screen.findByRole('heading', { level: 1 })).toBeInTheDocument()
  })

  beforeEach(() => {
    // Guarded routes need a session; everything is signed in as an Owner.
    localStorage.setItem('rng-token', 'test-token')
    vi.stubGlobal(
      'fetch',
      vi.fn(async (url: string) =>
        url.endsWith('/auth/me')
          ? new Response(JSON.stringify(owner), { status: 200 })
          : new Response(null, { status: 404 }),
      ),
    )
  })
  afterEach(() => {
    localStorage.clear()
    vi.unstubAllGlobals()
  })

  it.each(cases)('%s renders', async (path, heading) => {
    const router = createMemoryRouter(routes, { initialEntries: [path] })
    render(<Providers router={router} />)
    expect(await screen.findByRole('heading', { level: 1, name: heading })).toBeInTheDocument()
  })
})
