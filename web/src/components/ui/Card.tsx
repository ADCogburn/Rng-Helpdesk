import type { ComponentProps, ReactNode } from 'react'
import { cn } from '@/lib/cn'

export interface CardProps extends ComponentProps<'div'> {
  /** `none` is for cards that contain a full-bleed table. */
  padding?: 'none' | 'sm' | 'md' | 'lg'
  /** Adds a hover lift; use for clickable/featured cards. */
  interactive?: boolean
}

const paddings = { none: '', sm: 'p-4', md: 'p-5 sm:p-6', lg: 'p-6 sm:p-8' }

export function Card({ padding = 'md', interactive, className, ...props }: CardProps) {
  return (
    <div
      className={cn(
        'border-line bg-surface rounded-xl border',
        paddings[padding],
        interactive &&
          'hover:border-primary/50 transition hover:-translate-y-0.5 hover:shadow-lg hover:shadow-black/20',
        className,
      )}
      {...props}
    />
  )
}

export interface CardHeaderProps {
  title: ReactNode
  description?: ReactNode
  /** Right-aligned actions (buttons). */
  actions?: ReactNode
  className?: string
}

export function CardHeader({ title, description, actions, className }: CardHeaderProps) {
  return (
    <div className={cn('mb-4 flex flex-wrap items-start justify-between gap-3', className)}>
      <div className="min-w-0">
        <h3 className="text-fg text-lg font-semibold">{title}</h3>
        {description && <p className="text-muted mt-1 text-sm">{description}</p>}
      </div>
      {actions && <div className="flex shrink-0 items-center gap-2">{actions}</div>}
    </div>
  )
}
