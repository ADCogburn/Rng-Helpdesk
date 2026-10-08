import { Crown, Medal, Trophy } from 'lucide-react'
import { usePublicLeaderboard } from '@/api/hooks/public'
import type { LeaderboardEntry } from '@/api/types'
import { EmptyState, ErrorState, RankBadge, Skeleton } from '@/components/ui'
import { cn } from '@/lib/cn'
import { formatNumber } from '@/lib/format'
import { SectionHeading } from './SectionHeading'

const TOP = 25

const podium = {
  1: { color: '#e8c26a', label: 'First place', order: 'md:order-2', pad: 'md:py-10' },
  2: { color: '#c3cad3', label: 'Second place', order: 'md:order-1', pad: 'md:py-7' },
  3: { color: '#c9803f', label: 'Third place', order: 'md:order-3', pad: 'md:py-6' },
} as const

function PodiumCard({ entry }: { entry: LeaderboardEntry }) {
  const p = podium[entry.position as 1 | 2 | 3]
  const Icon = entry.position === 1 ? Crown : Medal
  return (
    <li
      data-position={entry.position}
      className={cn(
        'border-line bg-surface relative overflow-hidden rounded-xl border px-5 py-6 text-center transition hover:-translate-y-1',
        p.order,
        p.pad,
      )}
      style={{ borderColor: `color-mix(in srgb, ${p.color} 55%, var(--color-line))` }}
    >
      <div
        aria-hidden
        className="pointer-events-none absolute inset-0"
        style={{
          background: `radial-gradient(circle at 50% 0%, color-mix(in srgb, ${p.color} 22%, transparent), transparent 70%)`,
        }}
      />
      <div className="relative">
        <Icon
          aria-hidden
          className="mx-auto size-9"
          style={{ color: p.color, filter: `drop-shadow(0 0 8px ${p.color})` }}
          fill="currentColor"
          fillOpacity={0.2}
        />
        <p
          className="mt-1 text-xs font-semibold tracking-widest uppercase"
          style={{ color: p.color }}
        >
          <span className="sr-only">{p.label}, </span>#{entry.position}
        </p>
        <p className="font-display mt-2 truncate text-xl font-bold">{entry.runescapeUsername}</p>
        <div className="mt-2">
          <RankBadge rank={entry.rank} size="sm" />
        </div>
        <p className="mt-3 text-2xl font-semibold tabular-nums">{formatNumber(entry.clanPoints)}</p>
        <p className="text-muted text-xs">clan points</p>
      </div>
    </li>
  )
}

export function Leaderboard() {
  const query = usePublicLeaderboard(TOP)

  let body
  if (query.isPending) {
    body = (
      <div aria-busy="true" className="space-y-3">
        <div className="grid gap-3 md:grid-cols-3">
          {[0, 1, 2].map((i) => (
            <Skeleton key={i} className="h-44" />
          ))}
        </div>
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton key={i} className="h-12" />
        ))}
      </div>
    )
  } else if (query.isError) {
    body = (
      <ErrorState
        error={query.error}
        title="Couldn't load the leaderboard"
        onRetry={() => query.refetch()}
      />
    )
  } else if (query.data.entries.length === 0) {
    body = (
      <EmptyState
        icon={<Trophy aria-hidden />}
        title="No one on the board yet"
        description="Link your RuneScape account and start earning points to claim the first spot."
      />
    )
  } else {
    const entries = [...query.data.entries].sort((a, b) => a.position - b.position)
    const top = entries.filter((e) => e.position <= 3)
    const rest = entries.filter((e) => e.position > 3)
    body = (
      <div className="space-y-6">
        <ol className="grid items-end gap-3 md:grid-cols-3" aria-label="Top three">
          {top.map((e) => (
            <PodiumCard key={e.position} entry={e} />
          ))}
        </ol>
        {rest.length > 0 && (
          <ol className="border-line bg-surface divide-line divide-y overflow-hidden rounded-xl border">
            {rest.map((e) => (
              <li
                key={e.position}
                className="hover:bg-raised/60 flex items-center gap-3 px-4 py-3 transition sm:gap-4"
              >
                <span className="text-muted w-8 shrink-0 text-right text-sm font-medium tabular-nums">
                  {e.position}
                </span>
                <span className="min-w-0 flex-1 truncate font-medium">{e.runescapeUsername}</span>
                <RankBadge rank={e.rank} size="sm" className="hidden sm:inline-flex" />
                <span className="w-20 shrink-0 text-right font-semibold tabular-nums sm:w-24">
                  {formatNumber(e.clanPoints)}
                  <span className="text-muted ml-1 text-xs font-normal">pts</span>
                </span>
              </li>
            ))}
          </ol>
        )}
      </div>
    )
  }

  return (
    <section
      id="leaderboard"
      className="mx-auto max-w-4xl scroll-mt-20 px-4 py-16 sm:px-6 sm:py-20"
    >
      <SectionHeading
        eyebrow="Hall of fame"
        title="Leaderboard"
        description={`The top ${TOP} members by clan points.`}
      />
      {body}
    </section>
  )
}
