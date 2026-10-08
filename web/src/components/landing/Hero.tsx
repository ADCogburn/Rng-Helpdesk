import { ChevronDown, Coins, ShieldCheck, Users } from 'lucide-react'
import type { ReactNode } from 'react'
import { usePublicOverview } from '@/api/hooks/public'
import { POINT_RANKS } from '@/api/types'
import { Skeleton } from '@/components/ui'
import { buttonClass } from '@/components/ui/buttonStyles'
import { clan } from '@/config/clan'
import { formatNumber } from '@/lib/format'

const embers = [
  { left: '12%', top: '62%', size: 6, delay: '0s' },
  { left: '24%', top: '30%', size: 4, delay: '1.6s' },
  { left: '38%', top: '74%', size: 5, delay: '3.1s' },
  { left: '61%', top: '22%', size: 4, delay: '0.8s' },
  { left: '73%', top: '68%', size: 6, delay: '2.4s' },
  { left: '86%', top: '38%', size: 5, delay: '4s' },
]

function StatChip({ icon, label, value }: { icon: ReactNode; label: string; value?: string }) {
  return (
    <div className="border-line/70 bg-surface/70 flex items-center gap-3 rounded-xl border px-4 py-3 text-left backdrop-blur">
      <span className="bg-primary/10 text-primary grid size-9 place-items-center rounded-lg [&>svg]:size-5">
        {icon}
      </span>
      <div>
        <div className="text-fg text-lg leading-tight font-semibold tabular-nums">
          {value ?? <Skeleton className="h-5 w-14" />}
        </div>
        <div className="text-muted text-xs">{label}</div>
      </div>
    </div>
  )
}

export function Hero() {
  const overview = usePublicOverview()
  const data = overview.data

  return (
    <section id="top" className="relative isolate overflow-hidden">
      <div aria-hidden className="pointer-events-none absolute inset-0 -z-10">
        <div className="bg-primary/20 animate-ember absolute -top-24 left-1/2 h-[28rem] w-[44rem] max-w-[140%] -translate-x-1/2 rounded-full blur-3xl" />
        <div
          className="animate-ember absolute right-[-8rem] bottom-[-6rem] h-72 w-72 rounded-full blur-3xl"
          style={{
            backgroundColor: 'color-mix(in srgb, var(--color-danger) 18%, transparent)',
            animationDelay: '2s',
          }}
        />
        {embers.map((e, i) => (
          <span
            key={i}
            className="bg-primary-hover animate-ember absolute rounded-full opacity-60 blur-[1px]"
            style={{
              left: e.left,
              top: e.top,
              width: e.size,
              height: e.size,
              animationDelay: e.delay,
            }}
          />
        ))}
        <div className="to-bg absolute inset-x-0 bottom-0 h-32 bg-linear-to-b from-transparent" />
      </div>

      <div className="mx-auto max-w-4xl px-4 pt-20 pb-24 text-center sm:px-6 sm:pt-28 sm:pb-32">
        <p className="border-primary/30 bg-primary/10 text-primary mb-6 inline-flex items-center gap-2 rounded-full border px-3 py-1 text-xs font-semibold tracking-[0.2em] uppercase">
          Old School RuneScape clan
        </p>
        <h1 className="text-5xl font-bold tracking-wide sm:text-7xl">
          <span className="from-primary-hover to-primary bg-linear-to-b bg-clip-text text-transparent">
            {clan.name}
          </span>
        </h1>
        <p className="text-muted mx-auto mt-6 max-w-2xl text-lg sm:text-xl">{clan.tagline}</p>

        <div className="mt-9 flex flex-col items-center justify-center gap-3 sm:flex-row">
          <a
            href={clan.discordInviteUrl}
            target="_blank"
            rel="noreferrer"
            className={buttonClass('primary', 'lg', false, 'w-full sm:w-auto')}
          >
            Join our Discord
          </a>
          <a href="#ranks" className={buttonClass('secondary', 'lg', false, 'w-full sm:w-auto')}>
            Explore the ranks
          </a>
        </div>

        <div className="mt-14 grid grid-cols-1 gap-3 sm:grid-cols-3" aria-label="Clan statistics">
          <StatChip
            icon={<Users aria-hidden />}
            label="Active members"
            value={data ? formatNumber(data.activeMemberCount) : undefined}
          />
          <StatChip
            icon={<Coins aria-hidden />}
            label="Clan points earned"
            value={data ? formatNumber(data.totalClanPoints) : undefined}
          />
          <StatChip
            icon={<ShieldCheck aria-hidden />}
            label="Ranks to climb"
            value={String(POINT_RANKS.length)}
          />
        </div>
        {overview.isError && (
          <p role="status" className="text-muted mt-3 text-xs">
            Live stats are unavailable right now.
          </p>
        )}
        <a
          href="#about"
          aria-label="Scroll to about section"
          className="text-muted hover:text-primary mt-12 inline-block transition"
        >
          <ChevronDown aria-hidden className="size-6" />
        </a>
      </div>
    </section>
  )
}
