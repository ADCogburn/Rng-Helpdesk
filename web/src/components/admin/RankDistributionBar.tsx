import { rankColorVar, rankLabel } from '@/lib/ranks'
import type { RankCount } from './memberFilters'

/** Onyx is near-black and vanishes on the dark theme, so give it a visible stand-in. */
const colorFor = (rank: RankCount['rank']) =>
  rank === 'Onyx' ? 'var(--color-muted)' : rankColorVar(rank)

/** Stacked horizontal bar of members per rank, with a legend underneath. */
export function RankDistributionBar({ distribution }: { distribution: RankCount[] }) {
  const total = distribution.reduce((sum, d) => sum + d.count, 0)
  return (
    <div>
      <div
        role="img"
        aria-label={`Rank distribution of ${total} active members`}
        className="bg-raised flex h-4 w-full overflow-hidden rounded-full"
      >
        {distribution.map((d) => (
          <div
            key={d.rank}
            title={`${rankLabel(d.rank)}: ${d.count}`}
            style={{ width: `${(d.count / total) * 100}%`, backgroundColor: colorFor(d.rank) }}
          />
        ))}
      </div>
      <ul className="mt-4 grid grid-cols-2 gap-x-4 gap-y-2 text-sm sm:grid-cols-3 lg:grid-cols-4">
        {distribution.map((d) => (
          <li key={d.rank} className="flex items-center gap-2">
            <span
              aria-hidden
              className="size-2.5 shrink-0 rounded-full"
              style={{ backgroundColor: colorFor(d.rank) }}
            />
            <span className="truncate">{rankLabel(d.rank)}</span>
            <span className="text-muted ml-auto tabular-nums">{d.count}</span>
          </li>
        ))}
      </ul>
    </div>
  )
}
