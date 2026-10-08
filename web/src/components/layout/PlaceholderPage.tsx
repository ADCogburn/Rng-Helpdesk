import { Link } from 'react-router'
import { Card } from '@/components/ui'

/** Stand-in rendered by route files until their owning task (F2-F8) fills them in. */
export function PlaceholderPage({
  title,
  task,
  children,
}: {
  title: string
  task: string
  children?: React.ReactNode
}) {
  return (
    <main className="mx-auto max-w-3xl p-6">
      <Card>
        <p className="text-primary text-xs tracking-widest uppercase">Placeholder · {task}</p>
        <h1 className="mt-2 text-3xl font-semibold">{title}</h1>
        <p className="text-muted mt-2">This page has not been built yet.</p>
        {children}
        <Link to="/" className="text-primary mt-4 inline-block text-sm underline">
          Back to home
        </Link>
      </Card>
    </main>
  )
}
