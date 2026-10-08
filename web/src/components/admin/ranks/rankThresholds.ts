import { POINT_RANKS, type PointRank } from '@/api/types'
import { formatNumber } from '@/lib/format'
import { rankLabel } from '@/lib/ranks'

export interface ThresholdBound {
  rank: PointRank
  pointsRequired: number
}

/** The saved thresholds of the ranks directly below and above a rank, when the API has them. */
export interface ThresholdNeighbours {
  previous?: ThresholdBound
  next?: ThresholdBound
}

export function neighboursOf(
  rank: PointRank,
  saved: ReadonlyMap<PointRank, number>,
): ThresholdNeighbours {
  const index = POINT_RANKS.indexOf(rank)
  const bound = (r: PointRank | undefined): ThresholdBound | undefined => {
    if (!r) return undefined
    const pointsRequired = saved.get(r)
    return pointsRequired === undefined ? undefined : { rank: r, pointsRequired }
  }
  return {
    previous: bound(POINT_RANKS[index - 1]),
    next: bound(POINT_RANKS[index + 1]),
  }
}

/** A non-negative whole number, or null when the draft is not one. */
export function parseThresholdDraft(draft: string): number | null {
  const text = draft.trim()
  if (!/^\d+$/.test(text)) return null
  const value = Number(text)
  return Number.isSafeInteger(value) ? value : null
}

/** Mirrors the server's ordering rule: strictly between the neighbouring ranks' thresholds. */
export function validateThreshold(draft: string, neighbours: ThresholdNeighbours): string | null {
  const value = parseThresholdDraft(draft)
  if (value === null) return 'Enter a whole number, 0 or more.'
  const { previous, next } = neighbours
  if (previous && value <= previous.pointsRequired)
    return `Must be more than ${rankLabel(previous.rank)} (${formatNumber(previous.pointsRequired)}).`
  if (next && value >= next.pointsRequired)
    return `Must be less than ${rankLabel(next.rank)} (${formatNumber(next.pointsRequired)}).`
  return null
}

/** Whether a draft is a different, valid value that can be saved. */
export function canSaveThreshold(
  draft: string,
  saved: number | undefined,
  neighbours: ThresholdNeighbours,
): boolean {
  const value = parseThresholdDraft(draft)
  return value !== null && value !== saved && validateThreshold(draft, neighbours) === null
}
