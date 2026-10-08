import { Info } from 'lucide-react'
import { errorMessage } from '@/api/errors'
import { useRankThresholds, useUpdateRankThreshold } from '@/api/hooks/admin'
import type { PointRank } from '@/api/types'
import {
  RankThresholdTable,
  RankThresholdTableSkeleton,
} from '@/components/admin/ranks/RankThresholdTable'
import { ErrorState, PageHeader, useToast } from '@/components/ui'
import { rankLabel } from '@/lib/ranks'

export function Component() {
  const thresholds = useRankThresholds()
  const update = useUpdateRankThreshold()
  const toast = useToast()

  const saved = new Map<PointRank, number>(
    (thresholds.data?.thresholds ?? []).map((t): [PointRank, number] => [t.rank, t.pointsRequired]),
  )

  const save = (rank: PointRank, pointsRequired: number) =>
    update.mutate(
      { rank, pointsRequired },
      {
        onSuccess: () => toast.success(`${rankLabel(rank)} threshold saved.`),
        onError: (e) => toast.error(errorMessage(e)),
      },
    )

  let body
  if (thresholds.isPending) {
    body = <RankThresholdTableSkeleton />
  } else if (thresholds.isError) {
    body = (
      <ErrorState
        error={thresholds.error}
        title="Couldn't load rank thresholds"
        onRetry={() => thresholds.refetch()}
      />
    )
  } else {
    body = (
      <RankThresholdTable
        saved={saved}
        savingRank={update.isPending ? update.variables?.rank : undefined}
        onSave={save}
      />
    )
  }

  return (
    <>
      <PageHeader
        title="Rank thresholds"
        description="Points a member needs to reach each point-based rank. Bronze is the floor and Zenyte the top."
      />

      <div className="border-line bg-raised/60 text-muted mb-4 flex items-start gap-3 rounded-lg border p-3 text-sm">
        <Info aria-hidden className="text-primary mt-0.5 size-4 shrink-0" />
        <p>
          Saved changes are stored right away, but rank resolution only uses them after the API
          restarts.
        </p>
      </div>

      {body}
    </>
  )
}
