import { QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { AppRole, GetUserResponse } from '@/api/types'
import { ToastProvider } from '@/components/ui'
import { createQueryClient } from '@/lib/queryClient'
import { routes } from '@/router'
import {
  canDemote,
  cumulativePoints,
  canPromote,
  isSelf,
  validatePointsForm,
  validateRsn,
} from './rules'

function member(id: string, appRole: AppRole = 'Member', over: Partial<GetUserResponse> = {}) {
  return {
    id,
    appRole,
    clanPoints: 100,
    rank: 'Bronze',
    isActive: true,
    dateCreated: '2026-01-01T00:00:00Z',
    discordAccount: { discordId: id, username: `user${id}` },
    runescapeAccounts: [{ username: `Rsn${id}` }],
    ...over,
  } as GetUserResponse
}

describe('rules', () => {
  it('validates the points form', () => {
    expect(validatePointsForm('50', 'Event win')).toEqual({ points: 50, reason: 'Event win' })
    for (const bad of ['', '0', '-5', '1.5', 'abc', '1e3'])
      expect(validatePointsForm(bad, 'r').pointsError).toBeTruthy()
    expect(validatePointsForm('5', '   ').reasonError).toBeTruthy()
  })

  it('validates RSNs', () => {
    expect(validateRsn('Zezima')).toBeUndefined()
    expect(validateRsn('Iron-Man 1')).toBeUndefined()
    for (const bad of ['', ' lead', 'trail ', 'waytoolongname1', 'bad_char'])
      expect(validateRsn(bad)).toBeTruthy()
  })

  it('applies role rules and the self check', () => {
    expect(canPromote('Member')).toBe(true)
    expect(
      ['Administrator', 'SuperAdministrator', 'Owner'].some((r) => canPromote(r as AppRole)),
    ).toBe(false)
    expect(canDemote('Administrator')).toBe(true)
    expect(['Member', 'SuperAdministrator', 'Owner'].some((r) => canDemote(r as AppRole))).toBe(
      false,
    )
    expect(isSelf('1', '1')).toBe(true)
    expect(isSelf('1', '2')).toBe(false)
  })

  it('builds cumulative points oldest first', () => {
    const pts = cumulativePoints([
      { delta: -10, reason: 'b', occurredAt: '2026-02-01T00:00:00Z' },
      { delta: 50, reason: 'a', occurredAt: '2026-01-01T00:00:00Z' },
    ])
    expect(pts.map((p) => p.total)).toEqual([50, 40])
  })
})

describe('member detail page', () => {
  const viewer = member('99', 'Owner')
  let target: GetUserResponse

  beforeEach(() => localStorage.clear())
  afterEach(() => {
    localStorage.clear()
    vi.unstubAllGlobals()
  })

  const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status })

  function renderPage(path: string) {
    localStorage.setItem('rng-token', 'tok')
    vi.stubGlobal(
      'fetch',
      vi.fn(async (url: string) => {
        const u = String(url)
        if (u.endsWith('/auth/me')) return json(viewer)
        if (u.endsWith(`/users/${target.id}`)) return json(target)
        return json('Not found.', 404)
      }),
    )
    render(
      <QueryClientProvider client={createQueryClient()}>
        <ToastProvider>
          <RouterProvider router={createMemoryRouter(routes, { initialEntries: [path] })} />
        </ToastProvider>
      </QueryClientProvider>,
    )
  }

  it('offers promote for a Member but not demote', async () => {
    target = member('5')
    renderPage('/admin/members/5')
    expect(await screen.findByRole('button', { name: /Promote to Administrator/ })).toBeEnabled()
    expect(screen.queryByRole('button', { name: /Demote/ })).not.toBeInTheDocument()
  })

  it('offers demote for an Administrator but not promote', async () => {
    target = member('5', 'Administrator')
    renderPage('/admin/members/5')
    expect(await screen.findByRole('button', { name: /Demote to Member/ })).toBeEnabled()
    expect(screen.queryByRole('button', { name: /Promote/ })).not.toBeInTheDocument()
  })

  it('offers no role change for an Owner', async () => {
    target = member('5', 'Owner')
    renderPage('/admin/members/5')
    expect(await screen.findByRole('button', { name: 'Deactivate' })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /Promote|Demote/ })).not.toBeInTheDocument()
  })

  it('disables destructive actions on your own account', async () => {
    target = member('99', 'Administrator')
    renderPage('/admin/members/99')
    expect(await screen.findByRole('button', { name: /Demote to Member/ })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Deactivate' })).toBeDisabled()
    expect(screen.getByText(/can't do this to your own account/)).toBeInTheDocument()
  })

  it('shows points form validation errors without calling the API', async () => {
    target = member('5')
    renderPage('/admin/members/5')
    await userEvent.type(await screen.findByLabelText(/^Points/), '-3')
    await userEvent.click(screen.getByRole('button', { name: 'Add points' }))
    expect(await screen.findByText('Enter a positive whole number.')).toBeInTheDocument()
    expect(screen.getByText('A reason is required.')).toBeInTheDocument()
  })

  it('shows a friendly not-found state with a link back', async () => {
    target = member('5')
    renderPage('/admin/members/404')
    expect(await screen.findByRole('heading', { name: 'Member not found' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Back to Members' })).toHaveAttribute(
      'href',
      '/admin/members',
    )
  })
})
