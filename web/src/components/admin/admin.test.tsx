import { QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { AppRole, GetUserResponse, Rank } from '@/api/types'
import { routes } from '@/router'
import { ToastProvider } from '@/components/ui'
import { createQueryClient } from '@/lib/queryClient'
import {
  dashboardStats,
  filterMembers,
  newestMembers,
  parseMemberParams,
  rankDistribution,
  serializeMemberParams,
  sortMembers,
  type MemberFilters,
} from './memberFilters'

function member(
  id: string,
  rsns: string[],
  over: Partial<GetUserResponse> & { discord?: string } = {},
): GetUserResponse {
  const { discord, ...rest } = over
  return {
    id,
    appRole: 'Member',
    clanPoints: 0,
    rank: 'Bronze',
    isActive: true,
    dateCreated: '2026-01-01T00:00:00Z',
    discordAccount: { discordId: id, username: discord ?? `user${id}` },
    runescapeAccounts: rsns.map((username) => ({ username })),
    ...rest,
  }
}

const none: MemberFilters = { q: '', rank: '', role: '', status: 'all' }

const roster = [
  member('1', ['Zezima', 'Old Zez'], { clanPoints: 500, rank: 'Iron', discord: 'Wiz' }),
  member('2', ['Alpha'], { clanPoints: 9000, rank: 'Mithril', appRole: 'Owner' as AppRole }),
  member('3', ['bravo'], { clanPoints: 50, isActive: false, appRole: 'Administrator' }),
  member('4', [], { clanPoints: 50, discord: 'NoRsn' }),
]

describe('filterMembers', () => {
  it('matches any RSN or the Discord name, case-insensitively', () => {
    expect(filterMembers(roster, { ...none, q: 'old z' }).map((u) => u.id)).toEqual(['1'])
    expect(filterMembers(roster, { ...none, q: 'WIZ' }).map((u) => u.id)).toEqual(['1'])
    expect(filterMembers(roster, { ...none, q: 'norsn' }).map((u) => u.id)).toEqual(['4'])
  })

  it('combines rank, role and status filters', () => {
    expect(filterMembers(roster, { ...none, rank: 'Mithril' as Rank }).map((u) => u.id)).toEqual([
      '2',
    ])
    expect(filterMembers(roster, { ...none, role: 'Administrator' }).map((u) => u.id)).toEqual([
      '3',
    ])
    expect(filterMembers(roster, { ...none, status: 'inactive' }).map((u) => u.id)).toEqual(['3'])
    expect(filterMembers(roster, { ...none, status: 'active' })).toHaveLength(3)
    expect(filterMembers(roster, { ...none, status: 'inactive', role: 'Owner' })).toHaveLength(0)
  })
})

describe('sortMembers', () => {
  it('sorts by points both ways with an RSN tie-break', () => {
    expect(sortMembers(roster, { key: 'points', dir: 'desc' }).map((u) => u.id)).toEqual([
      '2',
      '1',
      '3',
      '4',
    ])
    expect(sortMembers(roster, { key: 'points', dir: 'asc' }).map((u) => u.id)).toEqual([
      '3',
      '4',
      '1',
      '2',
    ])
  })

  it('sorts RSNs case-insensitively with RSN-less members last', () => {
    expect(sortMembers(roster, { key: 'rsn', dir: 'asc' }).map((u) => u.id)).toEqual([
      '2',
      '3',
      '1',
      '4',
    ])
  })

  it('sorts by ladder order for rank and privilege for role', () => {
    expect(sortMembers(roster, { key: 'rank', dir: 'desc' })[0]!.id).toBe('2')
    expect(sortMembers(roster, { key: 'role', dir: 'desc' })[0]!.appRole).toBe('Owner')
  })

  it('does not mutate its input', () => {
    const copy = [...roster]
    sortMembers(roster, { key: 'rsn', dir: 'desc' })
    expect(roster).toEqual(copy)
  })
})

describe('URL state', () => {
  it('round-trips filters and sort', () => {
    const filters: MemberFilters = { q: 'zez', rank: 'Iron', role: 'Owner', status: 'inactive' }
    const sort = { key: 'rsn', dir: 'asc' } as const
    const parsed = parseMemberParams(serializeMemberParams(filters, sort))
    expect(parsed).toEqual({ filters, sort })
  })

  it('omits defaults and ignores junk values', () => {
    expect(serializeMemberParams(none, { key: 'points', dir: 'desc' }).toString()).toBe('')
    const { filters, sort } = parseMemberParams(
      new URLSearchParams('rank=Nope&role=God&status=x&sort=bad'),
    )
    expect(filters).toEqual(none)
    expect(sort).toEqual({ key: 'points', dir: 'desc' })
  })
})

describe('dashboard helpers', () => {
  it('counts active/inactive/admins and sums active points only', () => {
    expect(dashboardStats(roster)).toEqual({
      active: 3,
      inactive: 1,
      admins: 1,
      totalPoints: 9550,
    })
  })

  it('builds a ladder-ordered distribution of active members', () => {
    expect(rankDistribution(roster)).toEqual([
      { rank: 'Bronze', count: 1 },
      { rank: 'Iron', count: 1 },
      { rank: 'Mithril', count: 1 },
    ])
  })

  it('returns the newest members first', () => {
    const users = [
      member('a', ['A'], { dateCreated: '2026-01-01T00:00:00Z' }),
      member('b', ['B'], { dateCreated: '2026-03-01T00:00:00Z' }),
      member('c', ['C'], { dateCreated: '2026-02-01T00:00:00Z' }),
    ]
    expect(newestMembers(users, 2).map((u) => u.id)).toEqual(['b', 'c'])
  })
})

describe('admin shell', () => {
  const admin = member('99', ['Boss'], { appRole: 'Owner', discord: 'bossman' })

  beforeEach(() => localStorage.clear())
  afterEach(() => {
    localStorage.clear()
    vi.unstubAllGlobals()
  })

  function renderAdmin(path: string, fetchImpl: (url: string) => Response) {
    localStorage.setItem('rng-token', 'tok')
    vi.stubGlobal(
      'fetch',
      vi.fn(async (url: string) => fetchImpl(String(url))),
    )
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

  const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status })

  const handler = (url: string) => {
    if (url.endsWith('/auth/me')) return json(admin)
    if (url.endsWith('/users')) return json({ totalCount: roster.length, users: roster })
    if (url.includes('/users/by-historical-rsn/')) return json({ users: [roster[0]] })
    return json('Not found.', 404)
  }

  it('renders the sidebar, current user and role badge, and the dashboard stats', async () => {
    renderAdmin('/admin', handler)
    expect(await screen.findByRole('heading', { name: 'Dashboard' })).toBeInTheDocument()
    const nav = screen.getByRole('navigation', { name: 'Admin' })
    for (const label of ['Dashboard', 'Members', 'Add member', 'Rank thresholds'])
      expect(nav).toHaveTextContent(label)
    expect(screen.getByText('bossman')).toBeInTheDocument()
    expect(screen.getByText('Owner', { selector: 'span.rounded-full' })).toBeInTheDocument()
    expect(await screen.findByText('Active members')).toBeInTheDocument()
    expect(screen.getByRole('img', { name: /Rank distribution/ })).toBeInTheDocument()
  })

  it('lists members and filters by URL state', async () => {
    renderAdmin('/admin/members?status=inactive', handler)
    expect(await screen.findByText('bravo')).toBeInTheDocument()
    expect(screen.queryByText('Zezima')).not.toBeInTheDocument()
    expect(screen.getByLabelText('Filter by status')).toHaveValue('inactive')
  })

  it('falls back to previous-RSN matches on Enter when nothing matches locally', async () => {
    renderAdmin('/admin/members', handler)
    const search = await screen.findByLabelText('Search members')
    await userEvent.type(search, 'Ancient Name{Enter}')
    expect(await screen.findByText(/previous RSN/)).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('Zezima')).toBeInTheDocument())
  })

  it('navigates to the member when a row is clicked', async () => {
    const router = renderAdmin('/admin/members', handler)
    await userEvent.click(await screen.findByText('Alpha'))
    await waitFor(() => expect(router.state.location.pathname).toBe('/admin/members/2'))
  })

  it('shows a friendly message when the quick search misses', async () => {
    renderAdmin('/admin', handler)
    await userEvent.type(await screen.findByLabelText('RuneScape name'), 'Nobody')
    await userEvent.click(screen.getByRole('button', { name: 'Find' }))
    expect(await screen.findByText(/No member found with the RSN "Nobody"/)).toBeInTheDocument()
  })
})
