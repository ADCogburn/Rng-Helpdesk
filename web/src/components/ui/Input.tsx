import type { ComponentProps } from 'react'
import { cn } from '@/lib/cn'
import { fieldClass } from './fieldStyles'

export interface InputProps extends ComponentProps<'input'> {
  invalid?: boolean
}

export function Input({ invalid, className, ...props }: InputProps) {
  return (
    <input
      aria-invalid={invalid || undefined}
      className={fieldClass(invalid, cn('h-10', className))}
      {...props}
    />
  )
}
