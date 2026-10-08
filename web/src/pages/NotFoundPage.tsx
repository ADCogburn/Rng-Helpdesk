import { Link } from 'react-router'
import { buttonClass } from '@/components/ui'

export function Component() {
  return (
    <main className="mx-auto flex min-h-screen max-w-md flex-col items-center justify-center gap-4 p-6 text-center">
      <p className="font-display text-primary text-6xl">404</p>
      <h1 className="text-2xl font-semibold">Page not found</h1>
      <p className="text-muted">The page you are looking for does not exist.</p>
      <Link to="/" className={buttonClass('primary')}>
        Back to home
      </Link>
    </main>
  )
}
