import type { ComponentProps } from 'react'
import { cn } from '@/lib/cn'

export interface LabelProps extends ComponentProps<'label'> {
  required?: boolean
}

export function Label({ required, className, children, ...props }: LabelProps) {
  return (
    <label className={cn('text-fg text-sm font-medium', className)} {...props}>
      {children}
      {required && (
        <span aria-hidden className="text-danger ml-0.5">
          *
        </span>
      )}
    </label>
  )
}
