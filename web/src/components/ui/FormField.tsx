import { useId, type ReactNode } from 'react'
import { cn } from '@/lib/cn'
import { Label } from './Label'

export interface FormFieldProps {
  label: string
  /** Receives the generated input id plus aria props; spread them onto Input/Select/Textarea. */
  children: (control: {
    id: string
    invalid: boolean
    'aria-describedby': string | undefined
  }) => ReactNode
  hint?: ReactNode
  error?: string | null
  required?: boolean
  className?: string
}

/** Label + control + hint/error, wired together with ids. */
export function FormField({ label, children, hint, error, required, className }: FormFieldProps) {
  const id = useId()
  const describedBy = error ? `${id}-error` : hint ? `${id}-hint` : undefined
  return (
    <div className={cn('flex flex-col gap-1.5', className)}>
      <Label htmlFor={id} required={required}>
        {label}
      </Label>
      {children({ id, invalid: !!error, 'aria-describedby': describedBy })}
      {error ? (
        <p id={`${id}-error`} role="alert" className="text-danger text-sm">
          {error}
        </p>
      ) : hint ? (
        <p id={`${id}-hint`} className="text-muted text-sm">
          {hint}
        </p>
      ) : null}
    </div>
  )
}
