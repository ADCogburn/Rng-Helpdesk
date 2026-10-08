import { Crown, LogIn } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router'
import { useLogin } from '@/api/hooks/auth'
import { CHANGE_PASSWORD_PATH, homePathFor, safeNext, useAuth } from '@/auth'
import { classifyLoginError, loginFailureMessage, type LoginFailure } from '@/auth/loginError'
import { Button, Card, FormField, Input, ThemeToggle } from '@/components/ui'
import { clan } from '@/config/clan'

export function Component() {
  const { status, user, mustChangePassword, signIn } = useAuth()
  const navigate = useNavigate()
  const [params] = useSearchParams()
  const next = safeNext(params.get('next'))
  const login = useLogin()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [failure, setFailure] = useState<LoginFailure | null>(null)
  const [submitting, setSubmitting] = useState(false)

  if (status === 'authenticated' && user && !submitting) {
    const target = mustChangePassword
      ? `${CHANGE_PASSWORD_PATH}${next ? `?next=${encodeURIComponent(next)}` : ''}`
      : (next ?? homePathFor(user.appRole))
    return <Navigate to={target} replace />
  }

  async function onSubmit(event: FormEvent) {
    event.preventDefault()
    if (!username.trim() || !password) {
      setFailure(null)
      return
    }
    setFailure(null)
    setSubmitting(true)
    try {
      const response = await login.mutateAsync({ username: username.trim(), password })
      const mustChange = response.mustChangePassword ?? false
      const me = await signIn(response.token, mustChange)
      const target = mustChange
        ? `${CHANGE_PASSWORD_PATH}${next ? `?next=${encodeURIComponent(next)}` : ''}`
        : (next ?? homePathFor(me.appRole))
      navigate(target, { replace: true })
    } catch (error) {
      setFailure(classifyLoginError(error))
      setSubmitting(false)
    }
  }

  const missing = !username.trim() || !password

  return (
    <main className="relative flex min-h-screen items-center justify-center overflow-hidden px-4 py-10">
      <div
        aria-hidden
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_at_top,var(--color-primary)_0%,transparent_55%)] opacity-10"
      />
      <div className="absolute top-4 right-4">
        <ThemeToggle />
      </div>
      <div className="relative w-full max-w-md">
        <div className="mb-6 text-center">
          <Crown aria-hidden className="text-primary mx-auto mb-3 size-10" />
          <h1 className="font-display text-fg text-3xl font-semibold tracking-wide">Sign in</h1>
          <p className="text-muted mt-1 text-sm">Welcome back to {clan.name}.</p>
        </div>
        <Card padding="lg" className="shadow-xl shadow-black/20">
          <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
            {failure && (
              <div
                role="alert"
                className="border-danger/40 bg-danger/10 text-danger rounded-lg border px-3 py-2 text-sm"
              >
                {loginFailureMessage[failure]}
              </div>
            )}
            <FormField label="Username" required>
              {(control) => (
                <Input
                  {...control}
                  name="username"
                  autoComplete="username"
                  autoFocus
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                />
              )}
            </FormField>
            <FormField label="Password" required>
              {(control) => (
                <Input
                  {...control}
                  name="password"
                  type="password"
                  autoComplete="current-password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
              )}
            </FormField>
            <Button
              type="submit"
              fullWidth
              loading={submitting}
              disabled={missing}
              leftIcon={<LogIn className="size-4" />}
            >
              Sign in
            </Button>
          </form>
        </Card>
        <p className="mt-6 text-center text-sm">
          <Link to="/" className="text-muted hover:text-primary underline-offset-4 hover:underline">
            Back to home
          </Link>
        </p>
      </div>
    </main>
  )
}
