import { useState } from 'react'
import { POINT_RANKS, type PointRank } from '@/api/types'
import {
  Button,
  Input,
  RankBadge,
  Skeleton,
  Table,
  TableBody,
  TableHead,
  Td,
  Th,
  Tr,
} from '@/components/ui'
import { formatNumber } from '@/lib/format'
import {
  canSaveThreshold,
  neighboursOf,
  parseThresholdDraft,
  validateThreshold,
  type ThresholdNeighbours,
} from './rankThresholds'

export interface RankThresholdTableProps {
  /** Saved `pointsRequired` per point rank, as returned by the API. */
  saved: ReadonlyMap<PointRank, number>
  /** The rank whose save is in flight, if any. */
  savingRank?: PointRank
  onSave: (rank: PointRank, pointsRequired: number) => void
}

function ThresholdRow({
  rank,
  saved,
  savingRank,
  neighbours,
  onSave,
}: {
  rank: PointRank
  saved: number | undefined
  savingRank?: PointRank
  neighbours: ThresholdNeighbours
  onSave: (rank: PointRank, pointsRequired: number) => void
}) {
  const [draft, setDraft] = useState(saved === undefined ? '' : String(saved))
  const touched = draft.trim() !== (saved === undefined ? '' : String(saved))
  const error = touched ? validateThreshold(draft, neighbours) : null
  const saving = savingRank === rank
  const canSave = !saving && canSaveThreshold(draft, saved, neighbours)
  const errorId = `threshold-${rank}-error`

  return (
    <Tr>
      <Td>
        <RankBadge rank={rank} size="sm" />
      </Td>
      <Td align="right" numeric>
        {saved === undefined ? <span className="text-muted">Not set</span> : formatNumber(saved)}
      </Td>
      <Td>
        <Input
          type="number"
          min={0}
          step={1}
          inputMode="numeric"
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          invalid={!!error}
          aria-label={`${rank} points required`}
          aria-describedby={error ? errorId : undefined}
          className="w-36 tabular-nums"
        />
        {error && (
          <p id={errorId} className="text-danger mt-1 text-xs">
            {error}
          </p>
        )}
      </Td>
      <Td align="right">
        <Button
          variant="secondary"
          size="sm"
          disabled={!canSave}
          loading={saving}
          onClick={() => {
            const value = parseThresholdDraft(draft)
            if (value !== null && canSave) onSave(rank, value)
          }}
        >
          Save
        </Button>
      </Td>
    </Tr>
  )
}

export function RankThresholdTable({ saved, savingRank, onSave }: RankThresholdTableProps) {
  return (
    <Table>
      <TableHead>
        <tr>
          <Th>Rank</Th>
          <Th align="right">Current points</Th>
          <Th>New threshold</Th>
          <Th align="right">
            <span className="sr-only">Actions</span>
          </Th>
        </tr>
      </TableHead>
      <TableBody>
        {POINT_RANKS.map((rank) => (
          <ThresholdRow
            // Remounting on a saved-value change resets the draft to the new server value.
            key={`${rank}:${saved.get(rank) ?? ''}`}
            rank={rank}
            saved={saved.get(rank)}
            savingRank={savingRank}
            neighbours={neighboursOf(rank, saved)}
            onSave={onSave}
          />
        ))}
      </TableBody>
    </Table>
  )
}

/** Placeholder rows shown while the thresholds load. */
export function RankThresholdTableSkeleton() {
  return (
    <div className="space-y-2" aria-busy="true">
      {Array.from({ length: POINT_RANKS.length }, (_, i) => (
        <Skeleton key={i} className="h-12" />
      ))}
    </div>
  )
}
