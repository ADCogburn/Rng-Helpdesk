import { useQueryClient } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router'
import { api } from '@/api/client'
import { ApiError } from '@/api/errors'
import { queryKeys } from '@/api/queryKeys'
import type { GetUserResponse } from '@/api/types'
import { Button, Input } from '@/components/ui'

/** Looks a member up by exact RSN and jumps to them; shows a friendly message on a miss. */
export function QuickRsnSearch() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [rsn, setRsn] = useState('')
  const [pending, setPending] = useState(false)
  const [message, setMessage] = useState<string | null>(null)

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    const term = rsn.trim()
    if (!term) return
    setPending(true)
    setMessage(null)
    try {
      const user = await queryClient.fetchQuery({
        queryKey: queryKeys.users.byRsn(term),
        queryFn: () => api.get<GetUserResponse>(`/users/by-rsn/${encodeURIComponent(term)}`),
        staleTime: 30_000,
      })
      await navigate(`/admin/members/${user.id}`)
    } catch (error) {
      setMessage(
        error instanceof ApiError && error.status === 404
          ? `No member found with the RSN "${term}". Try the members list to search by Discord name or a previous RSN.`
          : 'The search failed. Please try again.',
      )
    } finally {
      setPending(false)
    }
  }

  return (
    <form onSubmit={onSubmit} role="search" aria-label="Find member by RSN">
      <div className="flex gap-2">
        <Input
          value={rsn}
          onChange={(e) => setRsn(e.target.value)}
          placeholder="Exact RSN, e.g. Zezima"
          aria-label="RuneScape name"
          maxLength={12}
          autoComplete="off"
        />
        <Button
          type="submit"
          loading={pending}
          leftIcon={<Search aria-hidden className="size-4" />}
          disabled={!rsn.trim()}
        >
          Find
        </Button>
      </div>
      {message && (
        <p role="status" className="text-muted mt-2 text-sm">
          {message}
        </p>
      )}
    </form>
  )
}
