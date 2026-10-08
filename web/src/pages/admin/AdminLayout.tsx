import { LogOut, Menu } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Outlet } from 'react-router'
import { useAuth } from '@/auth'
import { AdminSidebar } from '@/components/admin/AdminSidebar'
import { RoleBadge } from '@/components/admin/RoleBadge'
import { Button, ThemeToggle } from '@/components/ui'

/** Admin app shell: fixed sidebar on desktop, drawer on mobile, top bar with the signed-in user. */
export function Component() {
  const { user, signOut } = useAuth()
  const [drawerOpen, setDrawerOpen] = useState(false)

  useEffect(() => {
    if (!drawerOpen) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setDrawerOpen(false)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [drawerOpen])

  const displayName =
    user?.discordAccount.username || user?.runescapeAccounts[0]?.username || 'Admin'

  return (
    <div className="bg-bg text-fg min-h-screen">
      <aside className="border-line bg-surface fixed inset-y-0 left-0 z-30 hidden w-60 border-r lg:block">
        <AdminSidebar />
      </aside>

      {drawerOpen && (
        <div className="fixed inset-0 z-50 lg:hidden">
          <div
            aria-hidden
            className="absolute inset-0 bg-black/60"
            onClick={() => setDrawerOpen(false)}
          />
          <aside
            role="dialog"
            aria-modal="true"
            aria-label="Navigation"
            className="border-line bg-surface absolute inset-y-0 left-0 w-64 max-w-[85vw] border-r shadow-xl"
          >
            <AdminSidebar showClose onNavigate={() => setDrawerOpen(false)} />
          </aside>
        </div>
      )}

      <div className="lg:pl-60">
        <header className="border-line/60 bg-bg/80 sticky top-0 z-20 flex h-16 items-center gap-3 border-b px-4 backdrop-blur-md sm:px-6">
          <button
            type="button"
            onClick={() => setDrawerOpen(true)}
            aria-label="Open navigation"
            className="text-muted hover:text-fg hover:bg-raised rounded-md p-2 lg:hidden"
          >
            <Menu aria-hidden className="size-5" />
          </button>
          <div className="flex-1" />
          <div className="flex min-w-0 items-center gap-2">
            <span
              className="hidden max-w-48 truncate text-sm font-medium sm:inline"
              title={displayName}
            >
              {displayName}
            </span>
            {user && <RoleBadge role={user.appRole} />}
          </div>
          <ThemeToggle />
          <Button
            variant="ghost"
            size="sm"
            onClick={signOut}
            leftIcon={<LogOut aria-hidden className="size-4" />}
          >
            <span className="hidden sm:inline">Sign out</span>
            <span className="sr-only sm:hidden">Sign out</span>
          </Button>
        </header>
        <main className="mx-auto max-w-7xl px-4 py-6 sm:px-6 sm:py-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
