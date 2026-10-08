import type { ComponentProps } from 'react'
import { cn } from '@/lib/cn'
import { fieldClass } from './fieldStyles'

export interface TextareaProps extends ComponentProps<'textarea'> {
  invalid?: boolean
}

export function Textarea({ invalid, rows = 3, className, ...props }: TextareaProps) {
  return (
    <textarea
      rows={rows}
      aria-invalid={invalid || undefined}
      className={fieldClass(invalid, cn('py-2', className))}
      {...props}
    />
  )
}
