import { CheckCircle2, Info, X, XCircle } from 'lucide-react'
import { useCallback, useMemo, useRef, useState, type ReactNode } from 'react'
import { cn } from '@/lib/cn'
import { ToastContext, type ToastApi, type ToastTone } from './toast-context'

interface ToastItem {
  id: number
  tone: ToastTone
  message: string
}

const icons = { success: CheckCircle2, error: XCircle, info: Info }
const tones: Record<ToastTone, string> = {
  success: 'border-success/50 [&>svg]:text-success',
  error: 'border-danger/50 [&>svg]:text-danger',
  info: 'border-line [&>svg]:text-primary',
}

export function ToastProvider({
  children,
  duration = 5000,
}: {
  children: ReactNode
  duration?: number
}) {
  const [toasts, setToasts] = useState<ToastItem[]>([])
  const nextId = useRef(1)

  const dismiss = useCallback((id: number) => setToasts((t) => t.filter((x) => x.id !== id)), [])

  const push = useCallback(
    (tone: ToastTone, message: string) => {
      const id = nextId.current++
      setToasts((t) => [...t, { id, tone, message }])
      window.setTimeout(() => dismiss(id), duration)
    },
    [dismiss, duration],
  )

  const api = useMemo<ToastApi>(
    () => ({
      success: (m) => push('success', m),
      error: (m) => push('error', m),
      info: (m) => push('info', m),
    }),
    [push],
  )

  return (
    <ToastContext.Provider value={api}>
      {children}
      <div
        aria-live="polite"
        className="pointer-events-none fixed right-4 bottom-4 left-4 z-[60] flex flex-col items-end gap-2 sm:left-auto"
      >
        {toasts.map((t) => {
          const Icon = icons[t.tone]
          return (
            <div
              key={t.id}
              role={t.tone === 'error' ? 'alert' : 'status'}
              className={cn(
                'animate-toast-in bg-raised pointer-events-auto flex w-full max-w-sm items-start gap-3 rounded-lg border p-3 text-sm shadow-xl',
                tones[t.tone],
              )}
            >
              <Icon aria-hidden className="mt-0.5 size-4 shrink-0" />
              <p className="text-fg flex-1">{t.message}</p>
              <button
                type="button"
                aria-label="Dismiss"
                onClick={() => dismiss(t.id)}
                className="text-muted hover:text-fg"
              >
                <X aria-hidden className="size-4" />
              </button>
            </div>
          )
        })}
      </div>
    </ToastContext.Provider>
  )
}
