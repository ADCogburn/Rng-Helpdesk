import { QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { CreateUserResponse } from '@/api/types'
import { ToastProvider } from '@/components/ui'
import { createQueryClient } from '@/lib/queryClient'
import { NewMemberForm } from './NewMemberForm'

const created: CreateUserResponse = {
  userId: '123456789012345678',
  username: 'member_42',
  temporaryPassword: 'Tmp-Passw0rd!',
}

function renderForm(onCreated = vi.fn()) {
  render(
    <QueryClientProvider client={createQueryClient()}>
      <ToastProvider>
        <NewMemberForm onCreated={onCreated} />
      </ToastProvider>
    </QueryClientProvider>,
  )
  return { onCreated }
}

afterEach(() => vi.unstubAllGlobals())

describe('NewMemberForm', () => {
  it('shows field errors only after a submit attempt', async () => {
    const user = userEvent.setup()
    const fetchMock = vi.fn()
    vi.stubGlobal('fetch', fetchMock)
    renderForm()

    expect(screen.queryByText('Discord ID is required.')).not.toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Create member' }))
    expect(screen.getByText('Discord ID is required.')).toBeInTheDocument()
    expect(screen.getByText('Discord username is required.')).toBeInTheDocument()
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('adds and removes RuneScape name rows', async () => {
    const user = userEvent.setup()
    renderForm()
    await user.click(screen.getByRole('button', { name: 'Add RuneScape name' }))
    await user.click(screen.getByRole('button', { name: 'Add RuneScape name' }))
    expect(screen.getByLabelText('RuneScape name 2')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Remove RuneScape name 1' }))
    expect(screen.queryByLabelText('RuneScape name 2')).not.toBeInTheDocument()
    expect(screen.getByLabelText('RuneScape name 1')).toBeInTheDocument()
  })

  it('posts the Discord ID as a string and shows the success callback', async () => {
    const user = userEvent.setup()
    const fetchMock = vi.fn(async () => new Response(JSON.stringify(created), { status: 201 }))
    vi.stubGlobal('fetch', fetchMock)
    const { onCreated } = renderForm()

    await user.type(screen.getByLabelText(/Discord ID/), '123456789012345678')
    await user.type(screen.getByLabelText(/Discord username/), 'wiz')
    await user.click(screen.getByRole('button', { name: 'Add RuneScape name' }))
    await user.type(screen.getByLabelText('RuneScape name 1'), 'Zezima')
    await user.click(screen.getByRole('button', { name: 'Create member' }))

    await waitFor(() => expect(onCreated).toHaveBeenCalledWith(created))
    const [url, init] = fetchMock.mock.calls[0] as unknown as [string, RequestInit]
    expect(url).toBe('/api/admin/create')
    expect(JSON.parse(String(init.body))).toEqual({
      discordAccount: { discordId: '123456789012345678', username: 'wiz' },
      runescapeAccounts: [{ username: 'Zezima' }],
    })
  })

  it('shows the API error message inline when creation fails', async () => {
    const user = userEvent.setup()
    vi.stubGlobal(
      'fetch',
      vi.fn(async () => new Response('Discord ID already exists.', { status: 400 })),
    )
    const { onCreated } = renderForm()

    await user.type(screen.getByLabelText(/Discord ID/), '123456789012345678')
    await user.type(screen.getByLabelText(/Discord username/), 'wiz')
    await user.click(screen.getByRole('button', { name: 'Create member' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Discord ID already exists.')
    expect(onCreated).not.toHaveBeenCalled()
  })
})
