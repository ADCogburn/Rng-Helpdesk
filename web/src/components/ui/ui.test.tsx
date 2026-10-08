import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useState } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { ApiError } from '@/api/errors'
import {
  Badge,
  Button,
  Card,
  ConfirmDialog,
  EmptyState,
  ErrorState,
  FormField,
  Input,
  RankBadge,
  Select,
  Skeleton,
  Spinner,
  Table,
  TableBody,
  TableHead,
  TabPanel,
  Tabs,
  Td,
  Textarea,
  Th,
  ToastProvider,
  Tr,
  useToast,
} from '.'

describe('ui primitives', () => {
  it('Button fires onClick and is disabled while loading', async () => {
    const onClick = vi.fn()
    const { rerender } = render(<Button onClick={onClick}>Save</Button>)
    await userEvent.click(screen.getByRole('button', { name: 'Save' }))
    expect(onClick).toHaveBeenCalledTimes(1)
    rerender(
      <Button onClick={onClick} loading>
        Save
      </Button>,
    )
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('renders Card, Badge, Skeleton, Spinner, RankBadge', () => {
    render(
      <>
        <Card>card body</Card>
        <Badge tone="success">Active</Badge>
        <Skeleton data-testid="sk" />
        <Spinner label="Fetching" />
        <RankBadge rank="DeputyOwner" />
      </>,
    )
    expect(screen.getByText('card body')).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
    expect(screen.getByRole('status')).toHaveTextContent('Fetching')
    expect(screen.getByText('Deputy Owner')).toBeInTheDocument()
  })

  it('FormField wires label, control and error', () => {
    render(
      <FormField label="Username" error="Required" required>
        {(c) => <Input {...c} />}
      </FormField>,
    )
    const input = screen.getByLabelText(/Username/)
    expect(input).toHaveAttribute('aria-invalid', 'true')
    expect(input).toHaveAccessibleDescription('Required')
  })

  it('Select and Textarea accept values', async () => {
    render(
      <>
        <Select aria-label="rank" defaultValue="b">
          <option value="a">A</option>
          <option value="b">B</option>
        </Select>
        <Textarea aria-label="notes" />
      </>,
    )
    await userEvent.type(screen.getByLabelText('notes'), 'hello')
    expect(screen.getByLabelText('notes')).toHaveValue('hello')
    expect(screen.getByLabelText('rank')).toHaveValue('b')
  })

  it('Table sort header calls onSort', async () => {
    const onSort = vi.fn()
    render(
      <Table>
        <TableHead>
          <tr>
            <Th onSort={onSort} sortDirection="asc">
              Points
            </Th>
          </tr>
        </TableHead>
        <TableBody>
          <Tr>
            <Td numeric>10</Td>
          </Tr>
        </TableBody>
      </Table>,
    )
    expect(screen.getByRole('columnheader')).toHaveAttribute('aria-sort', 'ascending')
    await userEvent.click(screen.getByRole('button', { name: /Points/ }))
    expect(onSort).toHaveBeenCalled()
  })

  it('Tabs switch panels and support arrow keys', async () => {
    function Harness() {
      const [v, setV] = useState('a')
      return (
        <>
          <Tabs
            value={v}
            onChange={setV}
            tabs={[
              { id: 'a', label: 'A' },
              { id: 'b', label: 'B' },
            ]}
          />
          <TabPanel id="a" value={v}>
            panel a
          </TabPanel>
          <TabPanel id="b" value={v}>
            panel b
          </TabPanel>
        </>
      )
    }
    render(<Harness />)
    expect(screen.getByText('panel a')).toBeInTheDocument()
    screen.getByRole('tab', { name: 'A' }).focus()
    await userEvent.keyboard('{ArrowRight}')
    expect(screen.getByText('panel b')).toBeInTheDocument()
  })

  it('ConfirmDialog confirms and cancels via Escape', async () => {
    const onConfirm = vi.fn()
    const onCancel = vi.fn()
    render(
      <ConfirmDialog
        open
        title="Deactivate?"
        tone="danger"
        onConfirm={onConfirm}
        onCancel={onCancel}
      />,
    )
    expect(screen.getByRole('dialog', { name: 'Deactivate?' })).toBeInTheDocument()
    await userEvent.click(screen.getByRole('button', { name: 'Confirm' }))
    expect(onConfirm).toHaveBeenCalled()
    await userEvent.keyboard('{Escape}')
    expect(onCancel).toHaveBeenCalled()
  })

  it('Toast shows a message', async () => {
    function Harness() {
      const toast = useToast()
      return <button onClick={() => toast.success('Saved!')}>go</button>
    }
    render(
      <ToastProvider>
        <Harness />
      </ToastProvider>,
    )
    await userEvent.click(screen.getByText('go'))
    await waitFor(() => expect(screen.getByText('Saved!')).toBeInTheDocument())
  })

  it('EmptyState and ErrorState render, ErrorState retries', async () => {
    const retry = vi.fn()
    render(
      <>
        <EmptyState title="Nothing here" description="Add something" />
        <ErrorState error={new ApiError(404, 'User not found')} onRetry={retry} />
      </>,
    )
    expect(screen.getByText('Nothing here')).toBeInTheDocument()
    expect(screen.getByText('User not found')).toBeInTheDocument()
    await userEvent.click(screen.getByRole('button', { name: /try again/i }))
    expect(retry).toHaveBeenCalled()
  })
})
