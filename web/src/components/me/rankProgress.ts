import type { PointRank, Rank, RankThreshold } from '@/api/types'
import { isPointRank } from '@/lib/ranks'

export type RankProgress =
  /** Admin-tier rank (Administrator/DeputyOwner/Owner): set by role, not by points. */
  | { status: 'role' }
  /** Top point-based rank reached; nothing left to climb to. */
  | { status: 'max' }
  /** The user's rank has no matching threshold (thresholds missing or out of date). */
  | { status: 'unknown' }
  | {
      status: 'next'
      nextRank: PointRank
      /** 0..1 share of the way from the current threshold to the next one. */
      fraction: number
      pointsToNext: number
    }

/**
 * Progress from the user's current point-based rank to the next one. `thresholds` must be in
 * ascending order, as `/public/ranks` returns them.
 */
export function rankProgress(
  clanPoints: number,
  rank: Rank,
  thresholds: readonly RankThreshold[],
): RankProgress {
  if (!isPointRank(rank)) return { status: 'role' }
  const index = thresholds.findIndex((t) => t.rank === rank)
  const current = thresholds[index]
  if (!current) return { status: 'unknown' }
  const next = thresholds[index + 1]
  if (!next) return { status: 'max' }

  const span = next.pointsRequired - current.pointsRequired
  const earned = clanPoints - current.pointsRequired
  const fraction = span > 0 ? Math.min(1, Math.max(0, earned / span)) : 1
  return {
    status: 'next',
    nextRank: next.rank,
    fraction,
    pointsToNext: Math.max(0, next.pointsRequired - clanPoints),
  }
}
