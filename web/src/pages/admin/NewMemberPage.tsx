import { useState } from 'react'
import { Link } from 'react-router'
import type { CreateUserResponse } from '@/api/types'
import { CreatedMemberPanel } from '@/components/admin/new-member/CreatedMemberPanel'
import { NewMemberForm } from '@/components/admin/new-member/NewMemberForm'
import { Card, PageHeader, buttonClass } from '@/components/ui'

export function Component() {
  const [created, setCreated] = useState<CreateUserResponse | null>(null)
  // Bumping the key remounts the form, which clears its draft and mutation state.
  const [formKey, setFormKey] = useState(0)

  return (
    <>
      <PageHeader
        title="Add member"
        description="Create a member with a temporary password they must change on first sign-in."
        actions={
          <Link to="/admin/members" className={buttonClass('secondary', 'md')}>
            Back to members
          </Link>
        }
      />

      <Card padding="lg" className="max-w-2xl">
        {created ? (
          <CreatedMemberPanel
            result={created}
            onAddAnother={() => {
              setCreated(null)
              setFormKey((k) => k + 1)
            }}
          />
        ) : (
          <NewMemberForm key={formKey} onCreated={setCreated} />
        )}
      </Card>
    </>
  )
}
