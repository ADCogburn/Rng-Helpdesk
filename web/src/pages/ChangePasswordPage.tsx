import { KeyRound, LogOut } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { useNavigate, useSearchParams } from 'react-router'
import { errorMessage } from '@/api/errors'
import { useChangePassword } from '@/api/hooks/auth'
import { homePathFor, safeNext, useAuth } from '@/auth'
import {
  MIN_PASSWORD_LENGTH,
  validatePasswordChange,
  type PasswordErrors,
} from '@/auth/passwordRules'
import { Button, Card, FormField, Input, useToast } from '@/components/ui'

export function Component() {
  const { user, mustChangePassword, passwordChanged, signOut } = useAuth()
  const navigate = useNavigate()
  const toast = useToast()
  const [params] = useSearchParams()
  const change = useChangePassword()
  const [current, setCurrent] = useState('')
  const [next, setNext] = useState('')
  const [confirm, setConfirm] = useState('')
  const [errors, setErrors] = useState<PasswordErrors>({})
  const [serverError, setServerError] = useState<string | null>(null)

  async function onSubmit(event: FormEvent) {
    event.preventDefault()
    setServerError(null)
    const found = validatePasswordChange(current, next, confirm)
    setErrors(found)
    if (Object.keys(found).length > 0) return
    try {
      await change.mutateAsync({ currentPassword: current, newPassword: next })
    } catch (error) {
      setServerError(errorMessage(error, 'Could not change your password.'))
      return
    }
    passwordChanged()
    toast.success('Password changed.')
    navigate(safeNext(params.get('next')) ?? homePathFor(user?.appRole), { replace: true })
  }

  return (
    <main className="flex min-h-screen items-center justify-center px-4 py-10">
      <div className="w-full max-w-md">
        <div className="mb-6 text-center">
          <KeyRound aria-hidden className="text-primary mx-auto mb-3 size-10" />
          <h1 className="font-display text-fg text-3xl font-semibold tracking-wide">
            Change password
          </h1>
          <p className="text-muted mt-1 text-sm">
            {mustChangePassword
              ? 'You are using a temporary password. Choose a new one to continue.'
              : 'Choose a new password for your account.'}
          </p>
        </div>
        <Card padding="lg" className="shadow-xl shadow-black/20">
          <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
            {serverError && (
              <div
                role="alert"
                className="border-danger/40 bg-danger/10 text-danger rounded-lg border px-3 py-2 text-sm"
              >
                {serverError}
              </div>
            )}
            <FormField label="Current password" required error={errors.currentPassword}>
              {(control) => (
                <Input
                  {...control}
                  type="password"
                  autoComplete="current-password"
                  autoFocus
                  value={current}
                  onChange={(e) => setCurrent(e.target.value)}
                />
              )}
            </FormField>
            <FormField
              label="New password"
              required
              error={errors.newPassword}
              hint={`At least ${MIN_PASSWORD_LENGTH} characters.`}
            >
              {(control) => (
                <Input
                  {...control}
                  type="password"
                  autoComplete="new-password"
                  value={next}
                  onChange={(e) => setNext(e.target.value)}
                />
              )}
            </FormField>
            <FormField label="Confirm new password" required error={errors.confirmPassword}>
              {(control) => (
                <Input
                  {...control}
                  type="password"
                  autoComplete="new-password"
                  value={confirm}
                  onChange={(e) => setConfirm(e.target.value)}
                />
              )}
            </FormField>
            <Button type="submit" fullWidth loading={change.isPending}>
              Change password
            </Button>
          </form>
        </Card>
        <p className="mt-6 text-center">
          <Button
            variant="ghost"
            size="sm"
            leftIcon={<LogOut className="size-4" />}
            onClick={() => {
              signOut()
              navigate('/login', { replace: true })
            }}
          >
            Sign out
          </Button>
        </p>
      </div>
    </main>
  )
}
