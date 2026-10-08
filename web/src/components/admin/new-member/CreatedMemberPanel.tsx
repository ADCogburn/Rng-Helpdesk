import { AlertTriangle, Check, CheckCircle2, Copy } from 'lucide-react'
import { useState } from 'react'
import { Link } from 'react-router'
import type { CreateUserResponse } from '@/api/types'
import { Button, buttonClass, useToast } from '@/components/ui'

function CopyButton({ value, label }: { value: string; label: string }) {
  const toast = useToast()
  const [copied, setCopied] = useState(false)

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(value)
      setCopied(true)
      window.setTimeout(() => setCopied(false), 2000)
    } catch {
      toast.error(`Couldn't copy the ${label.toLowerCase()}. Select it and copy it manually.`)
    }
  }

  return (
    <button
      type="button"
      onClick={copy}
      aria-label={`Copy ${label.toLowerCase()}`}
      className={buttonClass('secondary', 'sm')}
    >
      {copied ? (
        <Check aria-hidden className="text-success size-4" />
      ) : (
        <Copy aria-hidden className="size-4" />
      )}
      {copied ? 'Copied' : 'Copy'}
    </button>
  )
}

function CredentialRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="border-line bg-bg flex flex-col gap-2 rounded-lg border p-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="min-w-0">
        <dt className="text-muted text-xs tracking-wide uppercase">{label}</dt>
        <dd className="text-fg mt-1 font-mono text-base break-all">{value}</dd>
      </div>
      <CopyButton value={value} label={label} />
    </div>
  )
}

export interface CreatedMemberPanelProps {
  result: CreateUserResponse
  onAddAnother: () => void
}

/** Shown once after a member is created. The temporary password is not retrievable later. */
export function CreatedMemberPanel({ result, onAddAnother }: CreatedMemberPanelProps) {
  return (
    <div className="space-y-6">
      <div className="flex items-start gap-3">
        <CheckCircle2 aria-hidden className="text-success mt-0.5 size-6 shrink-0" />
        <div>
          <h2 className="text-fg text-xl font-semibold">Member created</h2>
          <p className="text-muted mt-1 text-sm">Give the member these sign-in details.</p>
        </div>
      </div>

      <dl className="space-y-3">
        <CredentialRow label="Username" value={result.username} />
        <CredentialRow label="Temporary password" value={result.temporaryPassword} />
      </dl>

      <div className="border-primary/40 bg-primary/10 text-fg flex items-start gap-3 rounded-lg border p-4 text-sm">
        <AlertTriangle aria-hidden className="text-primary mt-0.5 size-4 shrink-0" />
        <p>
          <strong>This password will not be shown again.</strong> The member must change it the
          first time they sign in.
        </p>
      </div>

      <div className="flex flex-wrap gap-3">
        <Link to={`/admin/members/${result.userId}`} className={buttonClass('primary', 'md')}>
          View member
        </Link>
        <Button variant="secondary" onClick={onAddAnother}>
          Add another
        </Button>
      </div>
    </div>
  )
}
