import type { ReactNode } from 'react'
import { Card } from './Card'
import { Skeleton } from './Skeleton'

export interface StatCardProps {
  label: string
  value: ReactNode
  icon?: ReactNode
  hint?: ReactNode
  /** Renders a skeleton instead of the value. */
  loading?: boolean
}

export function StatCard({ label, value, icon, hint, loading }: StatCardProps) {
  return (
    <Card padding="sm" className="flex items-start gap-4">
      {icon && (
        <div className="bg-primary/15 text-primary flex size-10 shrink-0 items-center justify-center rounded-lg [&>svg]:size-5">
          {icon}
        </div>
      )}
      <div className="min-w-0">
        <p className="text-muted text-sm">{label}</p>
        {loading ? (
          <Skeleton className="mt-1 h-8 w-24" />
        ) : (
          <p className="text-2xl font-semibold tabular-nums">{value}</p>
        )}
        {hint && <p className="text-muted mt-0.5 text-xs">{hint}</p>}
      </div>
    </Card>
  )
}
