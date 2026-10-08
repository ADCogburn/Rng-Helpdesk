import { Plus, Trash2 } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { errorMessage } from '@/api/errors'
import { useCreateUser } from '@/api/hooks/admin'
import type { CreateUserResponse } from '@/api/types'
import { Button, FormField, Input } from '@/components/ui'
import {
  emptyNewMemberDraft,
  hasErrors,
  toCreateUserRequest,
  validateNewMember,
  type NewMemberDraft,
} from './validation'

export interface NewMemberFormProps {
  onCreated: (result: CreateUserResponse) => void
}

/** Field errors appear after the first submit attempt, then update as the user types. */
export function NewMemberForm({ onCreated }: NewMemberFormProps) {
  const create = useCreateUser()
  const [draft, setDraft] = useState<NewMemberDraft>(emptyNewMemberDraft)
  const [submitted, setSubmitted] = useState(false)

  const errors = validateNewMember(draft)

  const setField = (field: 'discordId' | 'discordUsername', value: string) =>
    setDraft((d) => ({ ...d, [field]: value }))
  const setRsn = (index: number, value: string) =>
    setDraft((d) => ({ ...d, rsns: d.rsns.map((r, i) => (i === index ? value : r)) }))
  const addRsn = () => setDraft((d) => ({ ...d, rsns: [...d.rsns, ''] }))
  const removeRsn = (index: number) =>
    setDraft((d) => ({ ...d, rsns: d.rsns.filter((_, i) => i !== index) }))

  const onSubmit = (e: FormEvent) => {
    e.preventDefault()
    setSubmitted(true)
    if (hasErrors(errors)) return
    create.mutate(toCreateUserRequest(draft), { onSuccess: (result) => onCreated(result) })
  }

  return (
    <form onSubmit={onSubmit} noValidate className="space-y-6">
      {create.isError && (
        <div
          role="alert"
          className="border-danger/50 bg-danger/10 text-fg rounded-lg border p-3 text-sm"
        >
          {errorMessage(create.error, 'Could not create the member.')}
        </div>
      )}

      <fieldset className="space-y-5">
        <legend className="text-fg mb-3 text-base font-semibold">Discord</legend>
        <FormField
          label="Discord ID"
          required
          hint="The 17–20 digit numeric ID. Enable developer mode in Discord to copy it."
          error={submitted ? errors.discordId : null}
        >
          {(c) => (
            <Input
              {...c}
              value={draft.discordId}
              onChange={(e) => setField('discordId', e.target.value)}
              inputMode="numeric"
              autoComplete="off"
              spellCheck={false}
            />
          )}
        </FormField>
        <FormField
          label="Discord username"
          required
          error={submitted ? errors.discordUsername : null}
        >
          {(c) => (
            <Input
              {...c}
              value={draft.discordUsername}
              onChange={(e) => setField('discordUsername', e.target.value)}
              autoComplete="off"
            />
          )}
        </FormField>
      </fieldset>

      <fieldset className="space-y-3">
        <legend className="text-fg mb-3 text-base font-semibold">RuneScape names</legend>
        {draft.rsns.length === 0 && (
          <p className="text-muted text-sm">Optional. Add any RuneScape names this member uses.</p>
        )}
        {draft.rsns.map((rsn, i) => (
          <div key={i} className="flex items-end gap-2">
            <FormField
              label={`RuneScape name ${i + 1}`}
              className="min-w-0 flex-1"
              error={submitted ? errors.rsns[i] : null}
            >
              {(c) => (
                <Input
                  {...c}
                  value={rsn}
                  onChange={(e) => setRsn(i, e.target.value)}
                  maxLength={12}
                  autoComplete="off"
                  spellCheck={false}
                />
              )}
            </FormField>
            <Button
              variant="ghost"
              size="md"
              onClick={() => removeRsn(i)}
              aria-label={`Remove RuneScape name ${i + 1}`}
              className="shrink-0"
            >
              <Trash2 aria-hidden className="size-4" />
            </Button>
          </div>
        ))}
        <Button
          variant="secondary"
          onClick={addRsn}
          leftIcon={<Plus aria-hidden className="size-4" />}
        >
          Add RuneScape name
        </Button>
      </fieldset>

      <div className="flex flex-wrap items-center gap-3 pt-2">
        <Button type="submit" loading={create.isPending}>
          Create member
        </Button>
      </div>
    </form>
  )
}
