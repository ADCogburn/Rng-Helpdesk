import { render, screen, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { Component as LandingPage } from '@/pages/LandingPage'
import { POINT_RANKS } from '@/api/types'
import * as hooks from '@/api/hooks/public'

vi.mock('@/api/hooks/public')

const q = (over: Record<string, unknown>) =>
  ({
    isPending: false,
    isError: false,
    error: null,
    refetch: vi.fn(),
    data: undefined,
    ...over,
  }) as never

const ranksData = {
  ranks: POINT_RANKS.map((rank, i) => ({ rank, pointsRequired: i * 100 })),
}
const overviewData = {
  activeMemberCount: 42,
  totalClanPoints: 12345,
  rankDistribution: [{ rank: 'Bronze' as const, count: 7 }],
}
const entries = [1, 2, 3, 4, 5].map((position) => ({
  position,
  runescapeUsername: `Player ${position}`,
  rank: 'Rune' as const,
  clanPoints: 1000 - position,
}))

function renderPage() {
  return render(
    <MemoryRouter>
      <LandingPage />
    </MemoryRouter>,
  )
}

beforeEach(() => {
  vi.mocked(hooks.usePublicOverview).mockReturnValue(q({ data: overviewData }))
  vi.mocked(hooks.usePublicRanks).mockReturnValue(q({ data: ranksData }))
  vi.mocked(hooks.usePublicLeaderboard).mockReturnValue(q({ data: { entries } }))
})

describe('LandingPage', () => {
  it('renders stats, ladder with member counts, podium and join steps', () => {
    renderPage()
    expect(screen.getByRole('heading', { level: 1 })).toBeInTheDocument()
    expect(screen.getByText('12,345')).toBeInTheDocument()
    const ladder = document.getElementById('ranks')!
    expect(within(ladder).getAllByRole('listitem')).toHaveLength(POINT_RANKS.length)
    expect(within(ladder).getByText('7 members')).toBeInTheDocument()
    expect(screen.getByLabelText('Top three').children).toHaveLength(3)
    expect(screen.getByText('Player 5')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'How to join' })).toBeInTheDocument()
  })

  it('shows skeletons while loading', () => {
    vi.mocked(hooks.usePublicRanks).mockReturnValue(q({ isPending: true }))
    vi.mocked(hooks.usePublicLeaderboard).mockReturnValue(q({ isPending: true }))
    const { container } = renderPage()
    expect(container.querySelectorAll('[aria-busy="true"]')).toHaveLength(2)
  })

  it('shows retryable errors per section', () => {
    vi.mocked(hooks.usePublicRanks).mockReturnValue(q({ isError: true, error: new Error('x') }))
    vi.mocked(hooks.usePublicLeaderboard).mockReturnValue(
      q({ isError: true, error: new Error('x') }),
    )
    renderPage()
    expect(screen.getByText(/couldn't load the rank ladder/i)).toBeInTheDocument()
    expect(screen.getByText(/couldn't load the leaderboard/i)).toBeInTheDocument()
    expect(screen.getAllByRole('button', { name: /try again/i })).toHaveLength(2)
  })

  it('shows empty states', () => {
    vi.mocked(hooks.usePublicRanks).mockReturnValue(q({ data: { ranks: [] } }))
    vi.mocked(hooks.usePublicLeaderboard).mockReturnValue(q({ data: { entries: [] } }))
    renderPage()
    expect(screen.getByText('No ranks yet')).toBeInTheDocument()
    expect(screen.getByText('No one on the board yet')).toBeInTheDocument()
  })
})
