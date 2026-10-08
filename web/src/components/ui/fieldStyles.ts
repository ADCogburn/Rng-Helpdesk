import { cn } from '@/lib/cn'

/** Shared look for Input / Select / Textarea. */
export function fieldClass(invalid?: boolean, className?: string) {
  return cn(
    'w-full rounded-md border bg-bg px-3 text-sm text-fg placeholder:text-muted/70 transition',
    'focus-visible:border-primary focus-visible:outline-2 focus-visible:outline-primary focus-visible:outline-offset-0',
    'disabled:cursor-not-allowed disabled:opacity-50',
    invalid ? 'border-danger' : 'border-line hover:border-muted',
    className,
  )
}
