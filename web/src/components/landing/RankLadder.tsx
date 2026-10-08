import { Gem } from 'lucide-react'
import { usePublicOverview, usePublicRanks } from '@/api/hooks/public'
import { POINT_RANKS, type PointRank } from '@/api/types'
import { EmptyState, ErrorState, Skeleton } from '@/components/ui'
import { cn } from '@/lib/cn'
import { formatNumber } from '@/lib/format'
import { rankColorVar, rankLabel } from '@/lib/ranks'
import { SectionHeading } from './SectionHeading'

/** Onyx is near-black and vanishes on the dark theme, so give it a visible stand-in. */
function accentFor(rank: PointRank) {
  return rank === 'Onyx' ? 'var(--color-muted)' : rankColorVar(rank)
}

export function RankLadder() {
  const ranks = usePublicRanks()
  const overview = usePublicOverview()

  const counts = new Map(overview.data?.rankDistribution.map((d) => [d.rank, d.count]))

  let body
  if (ranks.isPending) {
    body = (
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4 xl:grid-cols-7" aria-busy="true">
        {POINT_RANKS.map((r) => (
          <Skeleton key={r} className="h-36" />
        ))}
      </div>
    )
  } else if (ranks.isError) {
    body = (
      <ErrorState
        error={ranks.error}
        title="Couldn't load the rank ladder"
        onRetry={() => ranks.refetch()}
      />
    )
  } else if (ranks.data.ranks.length === 0) {
    body = (
      <EmptyState
        icon={<Gem aria-hidden />}
        title="No ranks yet"
        description="The rank ladder hasn't been set up. Check back soon."
      />
    )
  } else {
    body = (
      <ol className="grid grid-cols-2 gap-3 lg:grid-cols-4 xl:grid-cols-7">
        {ranks.data.ranks.map((r, i) => {
          const accent = accentFor(r.rank)
          const count = counts.get(r.rank)
          return (
            <li
              key={r.rank}
              data-rank={r.rank}
              className={cn(
                'border-line bg-surface group relative overflow-hidden rounded-xl border p-4 text-center transition',
                'hover:-translate-y-1 hover:shadow-lg hover:shadow-black/20',
              )}
              style={{ ['--accent' as string]: accent }}
            >
              <div
                aria-hidden
                className="absolute inset-x-0 top-0 h-1"
                style={{ backgroundColor: accent }}
              />
              <div
                aria-hidden
                className="pointer-events-none absolute inset-0 opacity-60 transition group-hover:opacity-100"
                style={{
                  background: `radial-gradient(circle at 50% 0%, color-mix(in srgb, ${accent} 22%, transparent), transparent 65%)`,
                }}
              />
              <div className="relative">
                <p className="text-muted text-[0.7rem] tracking-widest uppercase tabular-nums">
                  Tier {i + 1}
                </p>
                <Gem
                  aria-hidden
                  className="mx-auto my-3 size-8"
                  style={{ color: accent, filter: `drop-shadow(0 0 8px ${accent})` }}
                  fill="currentColor"
                  fillOpacity={0.25}
                />
                <h3 className="font-display text-lg font-semibold">{rankLabel(r.rank)}</h3>
                <p className="text-muted mt-1 text-sm tabular-nums">
                  {r.pointsRequired === 0
                    ? 'Starting rank'
                    : `${formatNumber(r.pointsRequired)} pts`}
                </p>
                <p
                  className="mt-3 text-xs font-medium tabular-nums"
                  aria-label={count === undefined ? undefined : `${count} members`}
                >
                  {count === undefined ? (
                    <span className="text-muted">&mdash;</span>
                  ) : (
                    <span className="border-line bg-bg/60 rounded-full border px-2 py-0.5">
                      {formatNumber(count)} {count === 1 ? 'member' : 'members'}
                    </span>
                  )}
                </p>
              </div>
            </li>
          )
        })}
      </ol>
    )
  }

  return (
    <section id="ranks" className="bg-surface/40 border-line/60 scroll-mt-20 border-y">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 sm:py-20">
        <SectionHeading
          eyebrow="The ladder"
          title="Climb the ranks"
          description="Earn clan points by taking part. Every threshold you pass unlocks the next tier."
        />
        {body}
      </div>
    </section>
  )
}
