import { Crown, Gem, Shield } from 'lucide-react'
import type { Rank } from '@/api/types'
import { cn } from '@/lib/cn'
import { rankColorVar, rankIconKind, rankLabel } from '@/lib/ranks'

export interface RankBadgeProps {
  rank: Rank
  size?: 'sm' | 'md' | 'lg'
  /** Hide the icon (text-only chip). */
  hideIcon?: boolean
  className?: string
}

const sizes = {
  sm: 'px-2 py-0.5 text-xs gap-1',
  md: 'px-2.5 py-1 text-sm gap-1.5',
  lg: 'px-3.5 py-1.5 text-base gap-2',
}
const iconSizes = { sm: 'size-3', md: 'size-3.5', lg: 'size-4' }

const icons = { crown: Crown, shield: Shield, gem: Gem }

export function RankBadge({ rank, size = 'md', hideIcon, className }: RankBadgeProps) {
  const Icon = icons[rankIconKind(rank)]
  const color = rankColorVar(rank)
  return (
    <span
      data-rank={rank}
      className={cn(
        'inline-flex items-center rounded-full border font-medium whitespace-nowrap',
        sizes[size],
        className,
      )}
      style={{
        color: 'var(--rank-fg, var(--color-fg))',
        borderColor: `color-mix(in srgb, ${color} 70%, var(--color-line))`,
        backgroundColor: `color-mix(in srgb, ${color} 18%, transparent)`,
      }}
    >
      {!hideIcon && (
        <Icon
          aria-hidden
          className={iconSizes[size]}
          style={{ color }}
          fill="currentColor"
          fillOpacity={0.25}
        />
      )}
      {rankLabel(rank)}
    </span>
  )
}
