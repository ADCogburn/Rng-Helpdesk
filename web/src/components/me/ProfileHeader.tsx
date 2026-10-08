import type { GetUserResponse } from '@/api/types'
import { Badge, Card, RankBadge } from '@/components/ui'
import { formatDate } from '@/lib/format'
import { isAdminRole, rankColorVar, roleLabel } from '@/lib/ranks'

export interface ProfileHeaderProps {
  user: GetUserResponse
}

export function ProfileHeader({ user }: ProfileHeaderProps) {
  const name = user.discordAccount.username
  const isAdmin = isAdminRole(user.appRole)

  return (
    <Card padding="lg" className="relative overflow-hidden">
      <div
        aria-hidden
        className="pointer-events-none absolute inset-0 opacity-60"
        style={{
          background: `radial-gradient(circle at 0% 0%, color-mix(in srgb, ${rankColorVar(user.rank)} 22%, transparent), transparent 60%)`,
        }}
      />
      <div className="relative flex flex-col gap-6 sm:flex-row sm:items-center">
        <div
          aria-hidden
          className="font-display bg-raised grid size-20 shrink-0 place-items-center rounded-full border-2 text-3xl font-bold uppercase"
          style={{ borderColor: rankColorVar(user.rank) }}
        >
          {name.charAt(0)}
        </div>
        <div className="min-w-0 flex-1">
          <h2 className="font-display truncate text-3xl font-semibold tracking-wide sm:text-4xl">
            {name}
          </h2>
          <div className="mt-3 flex flex-wrap items-center gap-2">
            <RankBadge rank={user.rank} size="lg" />
            <Badge tone={isAdmin ? 'primary' : 'neutral'}>{roleLabel(user.appRole)}</Badge>
            <Badge tone={user.isActive ? 'success' : 'danger'}>
              {user.isActive ? 'Active' : 'Deactivated'}
            </Badge>
          </div>
          <p className="text-muted mt-3 text-sm">
            Member since <time dateTime={user.dateCreated}>{formatDate(user.dateCreated)}</time>
          </p>
        </div>
      </div>
    </Card>
  )
}
