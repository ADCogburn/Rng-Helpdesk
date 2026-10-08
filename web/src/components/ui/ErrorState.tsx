import { AlertTriangle, RefreshCw } from 'lucide-react'
import { errorMessage } from '@/api/errors'
import { cn } from '@/lib/cn'
import { Button } from './Button'

export interface ErrorStateProps {
  /** Error from a query/mutation; message is normalized via ApiError. */
  error?: unknown
  title?: string
  /** Overrides the message derived from `error`. */
  message?: string
  /** Shows a Retry button when provided (e.g. query.refetch). */
  onRetry?: () => void
  className?: string
}

export function ErrorState({
  error,
  title = 'Something went wrong',
  message,
  onRetry,
  className,
}: ErrorStateProps) {
  return (
    <div
      role="alert"
      className={cn(
        'border-danger/40 bg-danger/5 flex flex-col items-center justify-center rounded-xl border px-6 py-10 text-center',
        className,
      )}
    >
      <AlertTriangle aria-hidden className="text-danger mb-3 size-10" />
      <h3 className="text-lg font-semibold">{title}</h3>
      <p className="text-muted mt-1 max-w-md text-sm">{message ?? errorMessage(error)}</p>
      {onRetry && (
        <Button
          variant="secondary"
          size="sm"
          className="mt-4"
          leftIcon={<RefreshCw className="size-4" />}
          onClick={onRetry}
        >
          Try again
        </Button>
      )}
    </div>
  )
}
