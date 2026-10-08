import { Link } from 'react-router'
import { Swords } from 'lucide-react'
import { clan } from '@/config/clan'

export function LandingFooter() {
  return (
    <footer className="border-line/60 border-t">
      <div className="text-muted mx-auto flex max-w-6xl flex-col items-center justify-between gap-4 px-4 py-8 text-sm sm:flex-row sm:px-6">
        <div className="flex items-center gap-2">
          <Swords aria-hidden className="text-primary size-5" />
          <span className="font-display text-fg font-semibold">{clan.name}</span>
          <span>&copy; {new Date().getFullYear()}</span>
        </div>
        <ul className="flex items-center gap-5">
          <li>
            <a href="#ranks" className="hover:text-fg transition">
              Ranks
            </a>
          </li>
          <li>
            <a href="#leaderboard" className="hover:text-fg transition">
              Leaderboard
            </a>
          </li>
          <li>
            <a
              href={clan.discordInviteUrl}
              target="_blank"
              rel="noreferrer"
              className="hover:text-fg transition"
            >
              Discord
            </a>
          </li>
          <li>
            <Link to="/login" className="hover:text-fg transition">
              Staff sign in
            </Link>
          </li>
        </ul>
      </div>
      <p className="text-muted/70 pb-6 text-center text-xs">
        Not affiliated with Jagex. RuneScape is a trademark of Jagex Ltd.
      </p>
    </footer>
  )
}
