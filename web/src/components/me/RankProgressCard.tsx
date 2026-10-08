import { ArrowRight } from 'lucide-react'
import { usePublicRanks } from '@/api/hooks/public'
import type { Rank } from '@/api/types'
import { Card, CardHeader, ErrorState, RankBadge, Skeleton } from '@/components/ui'
import { formatNumber } from '@/lib/format'
import { rankColorVar, rankLabel } from '@/lib/ranks'
import { rankProgress, type RankProgress } from './rankProgress'

export interface RankProgressCardProps {
  clanPoints: number
  rank: Rank
}

/** Onyx is near-black and vanishes on the dark theme, so give it a visible stand-in. */
function accentFor(rank: Rank) {
  return rank === 'Onyx' ? 'var(--color-muted)' : rankColorVar(rank)
}

export function RankProgressCard({ clanPoints, rank }: RankProgressCardProps) {
  const ranks = usePublicRanks()

  let body
  if (ranks.isPending) {
    body = (
      <div aria-busy="true">
        <Skeleton className="h-3 w-full" />
      </div>
    )
  } else if (ranks.isError) {
    body = (
      <ErrorState
        error={ranks.error}
        title="Couldn't load your progress"
        onRetry={() => ranks.refetch()}
      />
    )
  } else {
    body = <Progress progress={rankProgress(clanPoints, rank, ranks.data.ranks)} />
  }

  return (
    <Card>
      <CardHeader
        title="Clan points"
        description="Points set your point-based rank. Admin roles override it."
      />
      <p className="font-display text-4xl font-semibold tabular-nums">{formatNumber(clanPoints)}</p>
      <div className="mt-6">{body}</div>
    </Card>
  )
}

function Progress({ progress }: { progress: RankProgress }) {
  if (progress.status === 'role') {
    return (
      <p className="text-muted text-sm">
        Your rank comes from your role, so there is no points progress to show.
      </p>
    )
  }
  if (progress.status === 'max') {
    return (
      <p className="text-muted text-sm">
        You have reached the highest point-based rank. Keep earning points to hold it.
      </p>
    )
  }
  if (progress.status === 'unknown') {
    return <p className="text-muted text-sm">Rank progress is not available right now.</p>
  }

  const { nextRank, fraction, pointsToNext } = progress
  const percent = Math.round(fraction * 100)
  const accent = accentFor(nextRank)
  return (
    <div>
      <div className="mb-2 flex items-center justify-between gap-3 text-sm">
        <span className="text-muted tabular-nums">{percent}%</span>
        <span className="flex items-center gap-1.5 tabular-nums">
          <span className="text-muted">
            {formatNumber(pointsToNext)} {pointsToNext === 1 ? 'point' : 'points'} to
          </span>
          <RankBadge rank={nextRank} size="sm" />
        </span>
      </div>
      <div
        role="progressbar"
        aria-label={`Progress to ${rankLabel(nextRank)}`}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-valuenow={percent}
        className="bg-raised border-line h-3 w-full overflow-hidden rounded-full border"
      >
        <div
          className="h-full rounded-full transition-[width] duration-700 ease-out motion-reduce:transition-none"
          style={{
            width: `${percent}%`,
            backgroundColor: accent,
            boxShadow: `0 0 12px ${accent}`,
          }}
        />
      </div>
      <p className="text-muted mt-3 flex items-center gap-1 text-xs">
        <ArrowRight aria-hidden className="size-3" />
        Next rank: {rankLabel(nextRank)}
      </p>
    </div>
  )
}
