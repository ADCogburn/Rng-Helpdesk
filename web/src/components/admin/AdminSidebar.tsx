import { Swords, X } from 'lucide-react'
import { NavLink, useLocation } from 'react-router'
import { clan } from '@/config/clan'
import { cn } from '@/lib/cn'
import { adminNav } from './adminNav'

/**
 * "Members" must not stay highlighted on /admin/members/new, where "Add member" is the active entry.
 */
function linkClass(isActive: boolean) {
  return cn(
    'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition',
    isActive ? 'bg-primary/15 text-primary' : 'text-muted hover:bg-raised hover:text-fg',
  )
}

export interface AdminSidebarProps {
  /** Called when a link is followed or the close button is pressed (closes the mobile drawer). */
  onNavigate?: () => void
  /** Shows the close button; only the mobile drawer needs it. */
  showClose?: boolean
}

export function AdminSidebar({ onNavigate, showClose }: AdminSidebarProps) {
  const { pathname } = useLocation()
  return (
    <div className="flex h-full flex-col">
      <div className="flex h-16 shrink-0 items-center justify-between px-4">
        <NavLink to="/" className="flex items-center gap-2" aria-label={`${clan.name} public site`}>
          <Swords aria-hidden className="text-primary size-6" />
          <span className="font-display text-xl font-bold tracking-wide">{clan.name}</span>
          <span className="text-muted text-xs tracking-widest uppercase">Admin</span>
        </NavLink>
        {showClose && (
          <button
            type="button"
            onClick={onNavigate}
            aria-label="Close navigation"
            className="text-muted hover:text-fg hover:bg-raised rounded-md p-2"
          >
            <X aria-hidden className="size-5" />
          </button>
        )}
      </div>
      <nav aria-label="Admin" className="flex-1 space-y-1 overflow-y-auto px-3 py-2">
        {adminNav.map(({ to, label, icon: Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            onClick={onNavigate}
            className={({ isActive }) =>
              linkClass(isActive && !(to === '/admin/members' && pathname === '/admin/members/new'))
            }
          >
            <Icon aria-hidden className="size-4.5" />
            {label}
          </NavLink>
        ))}
      </nav>
    </div>
  )
}
