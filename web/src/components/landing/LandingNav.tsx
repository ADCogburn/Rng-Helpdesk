import { Link } from 'react-router'
import { Swords } from 'lucide-react'
import { ThemeToggle } from '@/components/ui'
import { buttonClass } from '@/components/ui/buttonStyles'
import { clan } from '@/config/clan'

const links = [
  { href: '#ranks', label: 'Ranks' },
  { href: '#leaderboard', label: 'Leaderboard' },
  { href: '#join', label: 'Join' },
]

export function LandingNav() {
  return (
    <header className="border-line/60 bg-bg/80 sticky top-0 z-40 border-b backdrop-blur-md">
      <nav
        aria-label="Primary"
        className="mx-auto flex h-16 max-w-6xl items-center justify-between gap-4 px-4 sm:px-6"
      >
        <a href="#top" className="flex items-center gap-2" aria-label={`${clan.name} home`}>
          <Swords aria-hidden className="text-primary size-6" />
          <span className="font-display text-xl font-bold tracking-wide">{clan.name}</span>
        </a>
        <ul className="hidden items-center gap-1 md:flex">
          {links.map((l) => (
            <li key={l.href}>
              <a
                href={l.href}
                className="text-muted hover:text-fg hover:bg-raised rounded-md px-3 py-2 text-sm font-medium transition"
              >
                {l.label}
              </a>
            </li>
          ))}
        </ul>
        <div className="flex items-center gap-2">
          <ThemeToggle />
          <Link to="/login" className={buttonClass('secondary', 'sm')}>
            Sign in
          </Link>
        </div>
      </nav>
    </header>
  )
}
