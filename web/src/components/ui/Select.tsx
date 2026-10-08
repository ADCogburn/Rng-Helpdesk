import { ChevronDown } from 'lucide-react'
import type { ComponentProps } from 'react'
import { cn } from '@/lib/cn'
import { fieldClass } from './fieldStyles'

export interface SelectProps extends ComponentProps<'select'> {
  invalid?: boolean
}

/** Native <select>; pass <option> children. */
export function Select({ invalid, className, children, ...props }: SelectProps) {
  return (
    <div className="relative">
      <select
        aria-invalid={invalid || undefined}
        className={fieldClass(invalid, cn('h-10 appearance-none pr-9', className))}
        {...props}
      >
        {children}
      </select>
      <ChevronDown
        aria-hidden
        className="text-muted pointer-events-none absolute top-1/2 right-3 size-4 -translate-y-1/2"
      />
    </div>
  )
}
