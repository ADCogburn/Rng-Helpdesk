import type { ReactNode } from 'react'
import { cn } from '@/lib/cn'

export interface SectionHeadingProps {
  eyebrow?: string
  title: ReactNode
  description?: ReactNode
  className?: string
}

export function SectionHeading({ eyebrow, title, description, className }: SectionHeadingProps) {
  return (
    <div className={cn('mx-auto mb-10 max-w-2xl text-center sm:mb-12', className)}>
      {eyebrow && (
        <p className="text-primary mb-2 text-xs font-semibold tracking-[0.25em] uppercase">
          {eyebrow}
        </p>
      )}
      <h2 className="text-3xl font-bold sm:text-4xl">{title}</h2>
      <div
        aria-hidden
        className="via-primary/70 mx-auto mt-4 h-px w-24 bg-linear-to-r from-transparent to-transparent"
      />
      {description && <p className="text-muted mt-4 text-base sm:text-lg">{description}</p>}
    </div>
  )
}
