import type { GetUserResponse } from '@/api/types'
import { RoleBadge } from '@/components/admin/RoleBadge'
import { Badge, RankBadge } from '@/components/ui'
import { formatDate, formatNumber } from '@/lib/format'

export function MemberHeader({ user }: { user: GetUserResponse }) {
  const rsn = user.runescapeAccounts[0]?.username
  return (
    <div className="border-line bg-surface mb-6 flex flex-wrap items-start justify-between gap-4 rounded-xl border p-5">
      <div className="min-w-0">
        <h1 className="text-2xl font-semibold sm:text-3xl">
          {rsn ?? <span className="text-muted">No RSN</span>}
        </h1>
        <p className="text-muted mt-1 text-sm">
          {user.discordAccount.username} · Discord ID{' '}
          <span className="tabular-nums">{user.discordAccount.discordId}</span>
        </p>
        <div className="mt-3 flex flex-wrap items-center gap-2">
          <RankBadge rank={user.rank} />
          <RoleBadge role={user.appRole} />
          <Badge tone={user.isActive ? 'success' : 'danger'}>
            {user.isActive ? 'Active' : 'Inactive'}
          </Badge>
        </div>
      </div>
      <dl className="grid grid-cols-2 gap-x-8 gap-y-1 text-sm">
        <dt className="text-muted">Points</dt>
        <dd className="text-right font-semibold tabular-nums">{formatNumber(user.clanPoints)}</dd>
        <dt className="text-muted">Member since</dt>
        <dd className="text-right">{formatDate(user.dateCreated)}</dd>
      </dl>
    </div>
  )
}
