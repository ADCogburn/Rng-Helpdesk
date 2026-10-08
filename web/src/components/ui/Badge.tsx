import type { ComponentProps } from 'react'
import { cn } from '@/lib/cn'

export type BadgeTone = 'neutral' | 'primary' | 'success' | 'danger' | 'warning' | 'info'

export interface BadgeProps extends ComponentProps<'span'> {
  tone?: BadgeTone
}

const tones: Record<BadgeTone, string> = {
  neutral: 'bg-raised text-muted border-line',
  primary: 'bg-primary/15 text-primary border-primary/40',
  success: 'bg-success/15 text-success border-success/40',
  danger: 'bg-danger/15 text-danger border-danger/40',
  warning: 'bg-warning/15 text-warning border-warning/40',
  info: 'bg-rank-sapphire/15 text-rank-rune border-rank-rune/40',
}

export function Badge({ tone = 'neutral', className, ...props }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-xs font-medium whitespace-nowrap',
        tones[tone],
        className,
      )}
      {...props}
    />
  )
}
