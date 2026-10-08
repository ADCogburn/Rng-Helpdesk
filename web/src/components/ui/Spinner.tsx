import { Loader2 } from 'lucide-react'
import { cn } from '@/lib/cn'

export interface SpinnerProps {
  size?: 'sm' | 'md' | 'lg'
  /** Accessible label announced to screen readers. */
  label?: string
  className?: string
}

const sizes = { sm: 'size-4', md: 'size-6', lg: 'size-10' }

export function Spinner({ size = 'md', label = 'Loading', className }: SpinnerProps) {
  return (
    <span role="status" className={cn('inline-flex items-center', className)}>
      <Loader2 aria-hidden className={cn('text-primary animate-spin', sizes[size])} />
      <span className="sr-only">{label}</span>
    </span>
  )
}
