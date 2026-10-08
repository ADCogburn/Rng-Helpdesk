import type { ComponentProps } from 'react'
import { cn } from '@/lib/cn'

/** Pulsing placeholder block; size it with className (e.g. `h-6 w-32`). */
export function Skeleton({ className, ...props }: ComponentProps<'div'>) {
  return (
    <div aria-hidden className={cn('bg-raised animate-pulse rounded-md', className)} {...props} />
  )
}
