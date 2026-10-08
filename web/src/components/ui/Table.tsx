import { ArrowDown, ArrowUp, ChevronsUpDown } from 'lucide-react'
import type { ComponentProps } from 'react'
import { cn } from '@/lib/cn'

/** Horizontally scrollable table wrapper. Compose with TableHead/TableBody/Tr/Th/Td. */
export function Table({ className, ...props }: ComponentProps<'table'>) {
  return (
    <div className="border-line bg-surface w-full overflow-x-auto rounded-xl border">
      <table className={cn('w-full border-collapse text-left text-sm', className)} {...props} />
    </div>
  )
}

export function TableHead({ className, ...props }: ComponentProps<'thead'>) {
  return (
    <thead
      className={cn('bg-raised/60 text-muted text-xs tracking-wide uppercase', className)}
      {...props}
    />
  )
}

export function TableBody({ className, ...props }: ComponentProps<'tbody'>) {
  return <tbody className={cn('divide-line divide-y', className)} {...props} />
}

export function Tr({ className, ...props }: ComponentProps<'tr'>) {
  return <tr className={cn('hover:bg-raised/40 transition-colors', className)} {...props} />
}

export type SortDirection = 'asc' | 'desc'

export interface ThProps extends ComponentProps<'th'> {
  /** When provided the header becomes a sort button. */
  onSort?: () => void
  /** Current direction if this column is the sorted one, otherwise undefined/null. */
  sortDirection?: SortDirection | null
  align?: 'left' | 'right' | 'center'
}

const aligns = { left: 'text-left', right: 'text-right', center: 'text-center' }

export function Th({
  onSort,
  sortDirection,
  align = 'left',
  className,
  children,
  ...props
}: ThProps) {
  const Icon =
    sortDirection === 'asc' ? ArrowUp : sortDirection === 'desc' ? ArrowDown : ChevronsUpDown
  return (
    <th
      scope="col"
      aria-sort={
        sortDirection === 'asc' ? 'ascending' : sortDirection === 'desc' ? 'descending' : undefined
      }
      className={cn('px-4 py-3 font-semibold whitespace-nowrap', aligns[align], className)}
      {...props}
    >
      {onSort ? (
        <button
          type="button"
          onClick={onSort}
          className="hover:text-fg inline-flex items-center gap-1 uppercase"
        >
          {children}
          <Icon aria-hidden className={cn('size-3.5', !sortDirection && 'opacity-40')} />
        </button>
      ) : (
        children
      )}
    </th>
  )
}

export interface TdProps extends ComponentProps<'td'> {
  align?: 'left' | 'right' | 'center'
  /** Applies tabular-nums, for numeric columns. */
  numeric?: boolean
}

export function Td({ align = 'left', numeric, className, ...props }: TdProps) {
  return (
    <td
      className={cn('px-4 py-3 align-middle', aligns[align], numeric && 'tabular-nums', className)}
      {...props}
    />
  )
}
