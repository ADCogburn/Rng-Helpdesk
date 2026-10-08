import { Card, CardHeader, EmptyState, ErrorState, Skeleton } from '@/components/ui'
import { useUserLifecycle } from '@/api/hooks/users'
import { formatDateTime } from '@/lib/format'
import { Timeline } from './Timeline'

/** "UserCreated" -> "User created" */
const humanize = (action: string) =>
  action.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/^./, (c) => c.toUpperCase())

export function LifecycleTab({ id }: { id: string }) {
  const q = useUserLifecycle(id)
  return (
    <Card>
      <CardHeader title="Lifecycle" />
      {q.isPending ? (
        <Skeleton className="h-32" />
      ) : q.isError ? (
        <ErrorState error={q.error} onRetry={() => q.refetch()} />
      ) : q.data.history.length === 0 ? (
        <EmptyState title="No lifecycle events" />
      ) : (
        <Timeline
          items={q.data.history.map((h) => ({
            label: humanize(h.action),
            at: formatDateTime(h.occurredAt),
            sortKey: h.occurredAt,
          }))}
        />
      )}
    </Card>
  )
}
