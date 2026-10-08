import { ShieldCheck, ShieldOff, UserCheck, UserX } from 'lucide-react'
import { useState, type ReactNode } from 'react'
import {
  useDeactivateUser,
  useDemoteUser,
  usePromoteUser,
  useReactivateUser,
} from '@/api/hooks/admin'
import { useAddPoints, useRemovePoints } from '@/api/hooks/users'
import { errorMessage } from '@/api/errors'
import type { GetUserResponse } from '@/api/types'
import {
  Button,
  Card,
  CardHeader,
  ConfirmDialog,
  FormField,
  Input,
  Textarea,
  useToast,
} from '@/components/ui'
import { formatNumber } from '@/lib/format'
import { canDemote, canPromote, SELF_ACTION_HINT, validatePointsForm } from './rules'

type PointsMode = 'add' | 'remove'

export function PointsForm({ user }: { user: GetUserResponse }) {
  const toast = useToast()
  const add = useAddPoints(user.id)
  const remove = useRemovePoints(user.id)
  const [points, setPoints] = useState('')
  const [reason, setReason] = useState('')
  const [errors, setErrors] = useState<{ points?: string; reason?: string }>({})
  const [confirm, setConfirm] = useState<{ mode: PointsMode; points: number; reason: string }>()

  const run = (mode: PointsMode, body: { points: number; reason: string }) => {
    const m = mode === 'add' ? add : remove
    m.mutate(body, {
      onSuccess: () => {
        toast.success(
          `${mode === 'add' ? 'Added' : 'Removed'} ${formatNumber(body.points)} points.`,
        )
        setPoints('')
        setReason('')
        setConfirm(undefined)
      },
      onError: (e) => {
        toast.error(errorMessage(e))
        setConfirm(undefined)
      },
    })
  }

  const submit = (mode: PointsMode) => {
    const v = validatePointsForm(points, reason)
    setErrors({ points: v.pointsError, reason: v.reasonError })
    if (v.points === undefined || v.reason === undefined) return
    const body = { points: v.points, reason: v.reason }
    if (mode === 'remove') setConfirm({ mode, ...body })
    else run(mode, body)
  }

  return (
    <Card>
      <CardHeader title="Adjust points" description="Every change is recorded with its reason." />
      <form
        noValidate
        className="space-y-4"
        onSubmit={(e) => {
          e.preventDefault()
          submit('add')
        }}
      >
        <FormField label="Points" required error={errors.points}>
          {(c) => (
            <Input
              {...c}
              inputMode="numeric"
              value={points}
              onChange={(e) => setPoints(e.target.value)}
            />
          )}
        </FormField>
        <FormField label="Reason" required error={errors.reason}>
          {(c) => <Textarea {...c} value={reason} onChange={(e) => setReason(e.target.value)} />}
        </FormField>
        <div className="flex flex-wrap gap-2">
          <Button type="submit" loading={add.isPending}>
            Add points
          </Button>
          <Button variant="secondary" loading={remove.isPending} onClick={() => submit('remove')}>
            Remove points
          </Button>
        </div>
      </form>
      <ConfirmDialog
        open={!!confirm}
        tone="danger"
        title="Remove points?"
        description={`Remove ${confirm ? formatNumber(confirm.points) : ''} points from this member? Reason: ${confirm?.reason ?? ''}`}
        confirmLabel="Remove points"
        loading={remove.isPending}
        onCancel={() => setConfirm(undefined)}
        onConfirm={() =>
          confirm && run('remove', { points: confirm.points, reason: confirm.reason })
        }
      />
    </Card>
  )
}

type Action = 'promote' | 'demote' | 'deactivate' | 'reactivate'

const COPY: Record<Action, { title: string; description: string; label: string; done: string }> = {
  promote: {
    title: 'Promote to Administrator?',
    description: 'They will gain access to the admin console.',
    label: 'Promote',
    done: 'Promoted to Administrator.',
  },
  demote: {
    title: 'Demote to Member?',
    description: 'They will lose access to the admin console.',
    label: 'Demote',
    done: 'Demoted to Member.',
  },
  deactivate: {
    title: 'Deactivate this member?',
    description: 'They will be hidden from the public leaderboard and counts.',
    label: 'Deactivate',
    done: 'Member deactivated.',
  },
  reactivate: {
    title: 'Reactivate this member?',
    description: 'They will count towards the public leaderboard and totals again.',
    label: 'Reactivate',
    done: 'Member reactivated.',
  },
}

function Guarded({ disabled, children }: { disabled: boolean; children: ReactNode }) {
  return <span title={disabled ? SELF_ACTION_HINT : undefined}>{children}</span>
}

export function AccountActions({ user, viewerId }: { user: GetUserResponse; viewerId?: string }) {
  const toast = useToast()
  const mutations = {
    promote: usePromoteUser(),
    demote: useDemoteUser(),
    deactivate: useDeactivateUser(),
    reactivate: useReactivateUser(),
  }
  const [pending, setPending] = useState<Action>()
  const self = viewerId === user.id

  const confirm = () => {
    if (!pending) return
    const action = pending
    mutations[action].mutate(user.id, {
      onSuccess: () => toast.success(COPY[action].done),
      onError: (e) => toast.error(errorMessage(e)),
      onSettled: () => setPending(undefined),
    })
  }

  const showPromote = canPromote(user.appRole)
  const showDemote = canDemote(user.appRole)

  return (
    <Card>
      <CardHeader title="Role & status" />
      <div className="flex flex-wrap gap-2">
        {showPromote && (
          <Guarded disabled={self}>
            <Button
              variant="secondary"
              disabled={self}
              leftIcon={<ShieldCheck className="size-4" />}
              onClick={() => setPending('promote')}
            >
              Promote to Administrator
            </Button>
          </Guarded>
        )}
        {showDemote && (
          <Guarded disabled={self}>
            <Button
              variant="secondary"
              disabled={self}
              leftIcon={<ShieldOff className="size-4" />}
              onClick={() => setPending('demote')}
            >
              Demote to Member
            </Button>
          </Guarded>
        )}
        {user.isActive ? (
          <Guarded disabled={self}>
            <Button
              variant="danger"
              disabled={self}
              leftIcon={<UserX className="size-4" />}
              onClick={() => setPending('deactivate')}
            >
              Deactivate
            </Button>
          </Guarded>
        ) : (
          <Button
            variant="secondary"
            leftIcon={<UserCheck className="size-4" />}
            onClick={() => setPending('reactivate')}
          >
            Reactivate
          </Button>
        )}
      </div>
      {self && <p className="text-muted mt-3 text-sm">{SELF_ACTION_HINT} Ask another admin.</p>}
      {!showPromote && !showDemote && (
        <p className="text-muted mt-3 text-sm">
          This member&apos;s role can&apos;t be changed from here.
        </p>
      )}
      {pending && (
        <ConfirmDialog
          open
          tone={pending === 'reactivate' || pending === 'promote' ? 'default' : 'danger'}
          title={COPY[pending].title}
          description={COPY[pending].description}
          confirmLabel={COPY[pending].label}
          loading={mutations[pending].isPending}
          onCancel={() => setPending(undefined)}
          onConfirm={confirm}
        />
      )}
    </Card>
  )
}

export function OverviewTab({ user, viewerId }: { user: GetUserResponse; viewerId?: string }) {
  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <PointsForm user={user} />
      <AccountActions user={user} viewerId={viewerId} />
    </div>
  )
}
