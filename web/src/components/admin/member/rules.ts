import type { AppRole, PointHistoryEvent } from '@/api/types'

/** RSN rule from docs/ui/PLAN.md. */
export const RSN_PATTERN = /^(?! )[A-Za-z0-9 -]{1,12}(?<! )$/

export function validateRsn(raw: string): string | undefined {
  if (!raw) return 'Enter a RuneScape name.'
  if (!RSN_PATTERN.test(raw))
    return 'Use 1-12 letters, numbers, spaces or hyphens, with no leading or trailing space.'
  return undefined
}

export interface PointsInput {
  points?: number
  reason?: string
  pointsError?: string
  reasonError?: string
}

/** Points must be a positive whole number and the reason is required. */
export function validatePointsForm(rawPoints: string, rawReason: string): PointsInput {
  const out: PointsInput = {}
  const p = rawPoints.trim()
  if (!/^\d+$/.test(p) || !Number.isSafeInteger(Number(p)) || Number(p) <= 0)
    out.pointsError = 'Enter a positive whole number.'
  else out.points = Number(p)
  const reason = rawReason.trim()
  if (!reason) out.reasonError = 'A reason is required.'
  else out.reason = reason
  return out
}

export const isSelf = (viewerId: string | undefined, memberId: string) => viewerId === memberId

/** Only plain Members can be promoted. */
export const canPromote = (role: AppRole) => role === 'Member'
/** Only Administrators can be demoted; Owner/SuperAdministrator are never changed here. */
export const canDemote = (role: AppRole) => role === 'Administrator'

export const SELF_ACTION_HINT = "You can't do this to your own account."

export interface CumulativePoint {
  at: string
  total: number
}

/** Running total of deltas, oldest first. */
export function cumulativePoints(events: PointHistoryEvent[]): CumulativePoint[] {
  let total = 0
  return [...events]
    .sort((a, b) => a.occurredAt.localeCompare(b.occurredAt))
    .map((e) => {
      total += e.delta
      return { at: e.occurredAt, total }
    })
}
