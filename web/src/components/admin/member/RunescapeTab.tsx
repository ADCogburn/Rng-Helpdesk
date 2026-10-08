import { Check, Link2, Pencil, Unlink, X } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { errorMessage } from '@/api/errors'
import {
  useDelinkRunescapeAccount,
  useLinkRunescapeAccount,
  usePreviousRunescapeAccounts,
  useRenameRunescapeAccount,
  useRunescapeAccountHistory,
  useRunescapeAccounts,
} from '@/api/hooks/users'
import type { RunescapeAccountHistoryItem } from '@/api/types'
import {
  Button,
  Card,
  CardHeader,
  ConfirmDialog,
  EmptyState,
  ErrorState,
  FormField,
  Input,
  Skeleton,
  useToast,
} from '@/components/ui'
import { formatDateTime } from '@/lib/format'
import { validateRsn } from './rules'
import { Timeline } from './Timeline'

function AccountRow({ id, username }: { id: string; username: string }) {
  const toast = useToast()
  const rename = useRenameRunescapeAccount(id)
  const delink = useDelinkRunescapeAccount(id)
  const [editing, setEditing] = useState(false)
  const [value, setValue] = useState(username)
  const [error, setError] = useState<string>()
  const [confirmDelink, setConfirmDelink] = useState(false)

  const save = (e: FormEvent) => {
    e.preventDefault()
    const next = value.trim()
    const err = validateRsn(next)
    setError(err)
    if (err) return
    if (next === username) return setEditing(false)
    rename.mutate(
      { oldUsername: username, newUsername: next },
      {
        onSuccess: () => {
          toast.success(`Renamed ${username} to ${next}.`)
          setEditing(false)
        },
        onError: (e) => toast.error(errorMessage(e)),
      },
    )
  }

  return (
    <li className="border-line flex flex-wrap items-center justify-between gap-3 border-b py-3 last:border-b-0">
      {editing ? (
        <form noValidate onSubmit={save} className="flex flex-1 flex-wrap items-start gap-2">
          <FormField label={`New name for ${username}`} error={error} className="min-w-48 flex-1">
            {(c) => <Input {...c} value={value} onChange={(e) => setValue(e.target.value)} />}
          </FormField>
          <div className="flex gap-2 sm:mt-6">
            <Button
              type="submit"
              size="sm"
              loading={rename.isPending}
              leftIcon={<Check className="size-4" />}
            >
              Save
            </Button>
            <Button
              size="sm"
              variant="ghost"
              leftIcon={<X className="size-4" />}
              onClick={() => {
                setEditing(false)
                setValue(username)
                setError(undefined)
              }}
            >
              Cancel
            </Button>
          </div>
        </form>
      ) : (
        <>
          <span className="font-medium">{username}</span>
          <div className="flex gap-2">
            <Button
              size="sm"
              variant="secondary"
              aria-label={`Rename ${username}`}
              leftIcon={<Pencil className="size-4" />}
              onClick={() => setEditing(true)}
            >
              Rename
            </Button>
            <Button
              size="sm"
              variant="danger"
              aria-label={`Delink ${username}`}
              leftIcon={<Unlink className="size-4" />}
              onClick={() => setConfirmDelink(true)}
            >
              Delink
            </Button>
          </div>
        </>
      )}
      <ConfirmDialog
        open={confirmDelink}
        tone="danger"
        title="Delink RuneScape account?"
        description={`Remove ${username} from this member? It will move to their previous RSNs.`}
        confirmLabel="Delink"
        loading={delink.isPending}
        onCancel={() => setConfirmDelink(false)}
        onConfirm={() =>
          delink.mutate(
            { username },
            {
              onSuccess: () => toast.success(`Delinked ${username}.`),
              onError: (e) => toast.error(errorMessage(e)),
              onSettled: () => setConfirmDelink(false),
            },
          )
        }
      />
    </li>
  )
}

function LinkForm({ id }: { id: string }) {
  const toast = useToast()
  const link = useLinkRunescapeAccount(id)
  const [value, setValue] = useState('')
  const [error, setError] = useState<string>()

  const submit = (e: FormEvent) => {
    e.preventDefault()
    const rsn = value.trim()
    const err = validateRsn(rsn)
    setError(err)
    if (err) return
    link.mutate(
      { username: rsn },
      {
        onSuccess: () => {
          toast.success(`Linked ${rsn}.`)
          setValue('')
        },
        onError: (e) => toast.error(errorMessage(e)),
      },
    )
  }

  return (
    <form noValidate onSubmit={submit} className="mt-4 flex flex-wrap items-start gap-2">
      <FormField label="Link a RuneScape name" error={error} className="min-w-48 flex-1">
        {(c) => <Input {...c} value={value} onChange={(e) => setValue(e.target.value)} />}
      </FormField>
      <Button
        type="submit"
        loading={link.isPending}
        leftIcon={<Link2 className="size-4" />}
        className="sm:mt-6"
      >
        Link
      </Button>
    </form>
  )
}

function describe(item: RunescapeAccountHistoryItem): string {
  switch (item.changeType) {
    case 'Linked':
      return `Linked ${item.username ?? ''}`.trim()
    case 'Delinked':
      return `Delinked ${item.username ?? ''}`.trim()
    case 'Renamed':
      return `Renamed ${item.oldUsername ?? '?'} to ${item.newUsername ?? '?'}`
    default:
      return [item.changeType, item.username, item.oldUsername, item.newUsername]
        .filter(Boolean)
        .join(' ')
  }
}

export function RunescapeTab({ id }: { id: string }) {
  const current = useRunescapeAccounts(id)
  const previous = usePreviousRunescapeAccounts(id)
  const history = useRunescapeAccountHistory(id)

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <div className="space-y-6">
        <Card>
          <CardHeader title="Current RuneScape names" />
          {current.isPending ? (
            <Skeleton className="h-16" />
          ) : current.isError ? (
            <ErrorState error={current.error} onRetry={() => current.refetch()} />
          ) : current.data.accounts.length === 0 ? (
            <p className="text-muted text-sm">No RuneScape names linked.</p>
          ) : (
            <ul>
              {current.data.accounts.map((a) => (
                <AccountRow key={a.username} id={id} username={a.username} />
              ))}
            </ul>
          )}
          <LinkForm id={id} />
        </Card>
        <Card>
          <CardHeader title="Previous names" />
          {previous.isPending ? (
            <Skeleton className="h-10" />
          ) : previous.isError ? (
            <ErrorState error={previous.error} onRetry={() => previous.refetch()} />
          ) : previous.data.accounts.length === 0 ? (
            <p className="text-muted text-sm">No previous names.</p>
          ) : (
            <ul className="flex flex-wrap gap-2">
              {previous.data.accounts.map((a) => (
                <li key={a.username} className="bg-raised rounded-full px-3 py-1 text-sm">
                  {a.username}
                </li>
              ))}
            </ul>
          )}
        </Card>
      </div>
      <Card>
        <CardHeader title="History" />
        {history.isPending ? (
          <Skeleton className="h-32" />
        ) : history.isError ? (
          <ErrorState error={history.error} onRetry={() => history.refetch()} />
        ) : history.data.history.length === 0 ? (
          <EmptyState title="No history yet" />
        ) : (
          <Timeline
            items={history.data.history.map((h) => ({
              label: describe(h),
              at: formatDateTime(h.occurredAt),
              sortKey: h.occurredAt,
            }))}
          />
        )}
      </Card>
    </div>
  )
}
