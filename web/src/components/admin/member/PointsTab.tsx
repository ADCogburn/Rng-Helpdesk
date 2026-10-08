import { ArrowRight } from 'lucide-react'
import { usePointHistory } from '@/api/hooks/users'
import {
  Card,
  CardHeader,
  EmptyState,
  ErrorState,
  RankBadge,
  Skeleton,
  Table,
  TableBody,
  TableHead,
  Td,
  Th,
  Tr,
} from '@/components/ui'
import { formatDateTime, formatDelta } from '@/lib/format'
import { PointsChart } from './PointsChart'

export function PointsTab({ id }: { id: string }) {
  const q = usePointHistory(id)
  if (q.isPending) return <Skeleton className="h-64" />
  if (q.isError) return <ErrorState error={q.error} onRetry={() => q.refetch()} />
  const events = [...q.data.events].sort((a, b) => b.occurredAt.localeCompare(a.occurredAt))
  if (events.length === 0)
    return (
      <EmptyState
        title="No point history"
        description="No points have been added or removed yet."
      />
    )

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader title="Cumulative points" />
        <PointsChart events={q.data.events} />
      </Card>
      <Table>
        <TableHead>
          <tr>
            <Th align="right">Change</Th>
            <Th>Reason</Th>
            <Th>Rank</Th>
            <Th>Date</Th>
          </tr>
        </TableHead>
        <TableBody>
          {events.map((e, i) => (
            <Tr key={i}>
              <Td
                align="right"
                numeric
                className={e.delta < 0 ? 'text-danger font-medium' : 'text-success font-medium'}
              >
                {formatDelta(e.delta)}
              </Td>
              <Td>{e.reason}</Td>
              <Td>
                {e.rankBefore && e.rankAfter ? (
                  <span className="inline-flex items-center gap-2">
                    <RankBadge rank={e.rankBefore} size="sm" />
                    <ArrowRight aria-hidden className="text-muted size-3.5" />
                    <RankBadge rank={e.rankAfter} size="sm" />
                  </span>
                ) : (
                  <span className="text-muted">-</span>
                )}
              </Td>
              <Td className="whitespace-nowrap">{formatDateTime(e.occurredAt)}</Td>
            </Tr>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
