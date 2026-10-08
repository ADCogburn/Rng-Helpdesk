import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router'
import { describe, expect, it, vi } from 'vitest'
import type { CreateUserResponse } from '@/api/types'
import { ToastProvider } from '@/components/ui'
import { CreatedMemberPanel } from './CreatedMemberPanel'

const result: CreateUserResponse = {
  userId: '123456789012345678',
  username: 'member_42',
  temporaryPassword: 'Tmp-Passw0rd!',
}

function renderPanel(onAddAnother = vi.fn()) {
  render(
    <MemoryRouter>
      <ToastProvider>
        <CreatedMemberPanel result={result} onAddAnother={onAddAnother} />
      </ToastProvider>
    </MemoryRouter>,
  )
  return { onAddAnother }
}

describe('CreatedMemberPanel', () => {
  it('shows the generated username and temporary password', () => {
    renderPanel()
    expect(screen.getByText('member_42')).toBeInTheDocument()
    expect(screen.getByText('Tmp-Passw0rd!')).toBeInTheDocument()
  })

  it('warns that the password will not be shown again and must be changed', () => {
    renderPanel()
    expect(screen.getByText(/will not be shown again/)).toBeInTheDocument()
    expect(screen.getByText(/must change it the\s+first time they sign in/)).toBeInTheDocument()
  })

  it('copies each value to the clipboard', async () => {
    const user = userEvent.setup()
    renderPanel()
    await user.click(screen.getByRole('button', { name: 'Copy username' }))
    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Copy username' })).toHaveTextContent('Copied'),
    )
    expect(await navigator.clipboard.readText()).toBe('member_42')

    await user.click(screen.getByRole('button', { name: 'Copy temporary password' }))
    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Copy temporary password' })).toHaveTextContent(
        'Copied',
      ),
    )
    expect(await navigator.clipboard.readText()).toBe('Tmp-Passw0rd!')
  })

  it('links to the new member and offers to add another', async () => {
    const user = userEvent.setup()
    const { onAddAnother } = renderPanel()
    expect(screen.getByRole('link', { name: 'View member' })).toHaveAttribute(
      'href',
      `/admin/members/${result.userId}`,
    )
    await user.click(screen.getByRole('button', { name: 'Add another' }))
    expect(onAddAnother).toHaveBeenCalledTimes(1)
  })
})
