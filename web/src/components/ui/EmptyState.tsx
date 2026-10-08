import { Inbox } from 'lucide-react'
import type { ReactNode } from 'react'
import { cn } from '@/lib/cn'

export interface EmptyStateProps {
  title: string
  description?: ReactNode
  /** Defaults to an inbox icon. Pass a lucide icon element. */
  icon?: ReactNode
  /** Call-to-action, usually a Button. */
  action?: ReactNode
  className?: string
}

export function EmptyState({ title, description, icon, action, className }: EmptyStateProps) {
  return (
    <div
      className={cn(
        'border-line flex flex-col items-center justify-center rounded-xl border border-dashed px-6 py-12 text-center',
        className,
      )}
    >
      <div className="text-muted mb-3 [&>svg]:size-10">{icon ?? <Inbox aria-hidden />}</div>
      <h3 className="text-lg font-semibold">{title}</h3>
      {description && <p className="text-muted mt-1 max-w-md text-sm">{description}</p>}
      {action && <div className="mt-4">{action}</div>}
    </div>
  )
}
