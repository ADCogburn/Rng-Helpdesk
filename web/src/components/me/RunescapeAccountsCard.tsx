import { Swords } from 'lucide-react'
import type { RunescapeAccount } from '@/api/types'
import { Card, CardHeader, EmptyState } from '@/components/ui'

export interface RunescapeAccountsCardProps {
  accounts: RunescapeAccount[]
}

export function RunescapeAccountsCard({ accounts }: RunescapeAccountsCardProps) {
  return (
    <Card>
      <CardHeader
        title="RuneScape accounts"
        description="Characters linked to your Discord account."
      />
      {accounts.length === 0 ? (
        <EmptyState
          icon={<Swords aria-hidden />}
          title="No accounts linked"
          description="Ask a clan admin to link your RuneScape account."
        />
      ) : (
        <ul className="flex flex-col gap-2">
          {accounts.map((account) => (
            <li
              key={account.username}
              className="border-line bg-bg/40 flex items-center gap-3 rounded-lg border px-3 py-2.5"
            >
              <Swords aria-hidden className="text-primary size-4 shrink-0" />
              <span className="truncate font-medium">{account.username}</span>
            </li>
          ))}
        </ul>
      )}
    </Card>
  )
}
