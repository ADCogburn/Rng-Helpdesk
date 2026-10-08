import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import type { PointHistoryEvent } from '@/api/types'
import { cumulativePoints } from './rules'
import { formatDate, formatDateTime, formatNumber } from '@/lib/format'

export function PointsChart({ events }: { events: PointHistoryEvent[] }) {
  const data = cumulativePoints(events)
  return (
    <div role="img" aria-label="Cumulative points over time" className="h-64 w-full">
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={data} margin={{ top: 8, right: 16, bottom: 0, left: 0 }}>
          <CartesianGrid stroke="var(--color-line)" strokeDasharray="3 3" />
          <XAxis
            dataKey="at"
            tickFormatter={formatDate}
            stroke="var(--color-muted)"
            fontSize={12}
            minTickGap={32}
          />
          <YAxis
            stroke="var(--color-muted)"
            fontSize={12}
            tickFormatter={formatNumber}
            width={56}
          />
          <Tooltip
            labelFormatter={(v) => formatDateTime(String(v))}
            formatter={(v) => [formatNumber(Number(v)), 'Points']}
            contentStyle={{
              background: 'var(--color-raised)',
              border: '1px solid var(--color-line)',
              borderRadius: 8,
            }}
          />
          <Line
            type="monotone"
            dataKey="total"
            stroke="var(--color-primary)"
            strokeWidth={2}
            dot={data.length < 30}
            isAnimationActive={false}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  )
}
