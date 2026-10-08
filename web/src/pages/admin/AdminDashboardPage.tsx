import { ShieldCheck, Sparkles, UserMinus, Users } from 'lucide-react'
import { Link } from 'react-router'
import { useUsers } from '@/api/hooks/users'
import { QuickRsnSearch } from '@/components/admin/QuickRsnSearch'
import { RankDistributionBar } from '@/components/admin/RankDistributionBar'
import {
  dashboardStats,
  newestMembers,
  primaryRsn,
  rankDistribution,
} from '@/components/admin/memberFilters'
import {
  Card,
  CardHeader,
  EmptyState,
  ErrorState,
  PageHeader,
  RankBadge,
  Skeleton,
  StatCard,
} from '@/components/ui'
import { formatDate, formatNumber } from '@/lib/format'

export function Component() {
  const users = useUsers()
  const list = users.data?.users
  const stats = list ? dashboardStats(list) : null

  return (
    <>
      <PageHeader title="Dashboard" description="A snapshot of the clan." />

      {users.isError ? (
        <ErrorState
          error={users.error}
          title="Couldn't load members"
          onRetry={() => users.refetch()}
        />
      ) : (
        <div className="space-y-6">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <StatCard
              label="Active members"
              icon={<Users aria-hidden />}
              value={stats && formatNumber(stats.active)}
              loading={users.isPending}
            />
            <StatCard
              label="Total clan points"
              icon={<Sparkles aria-hidden />}
              value={stats && formatNumber(stats.totalPoints)}
              hint="Active members only"
              loading={users.isPending}
            />
            <StatCard
              label="Admins"
              icon={<ShieldCheck aria-hidden />}
              value={stats && formatNumber(stats.admins)}
              hint="Active, any elevated role"
              loading={users.isPending}
            />
            <StatCard
              label="Inactive"
              icon={<UserMinus aria-hidden />}
              value={stats && formatNumber(stats.inactive)}
              loading={users.isPending}
            />
          </div>

          <div className="grid gap-6 lg:grid-cols-3">
            <Card className="lg:col-span-2">
              <CardHeader title="Rank distribution" description="Active members per rank" />
              {users.isPending ? (
                <Skeleton className="h-16" />
              ) : rankDistribution(list!).length === 0 ? (
                <EmptyState title="No active members yet" />
              ) : (
                <RankDistributionBar distribution={rankDistribution(list!)} />
              )}
            </Card>
            <Card>
              <CardHeader title="Find a member" description="Jump straight to a profile by RSN" />
              <QuickRsnSearch />
            </Card>
          </div>

          <Card>
            <CardHeader
              title="Newest members"
              actions={
                <Link to="/admin/members" className="text-primary text-sm hover:underline">
                  View all
                </Link>
              }
            />
            {users.isPending ? (
              <div className="space-y-2">
                <Skeleton className="h-10" />
                <Skeleton className="h-10" />
                <Skeleton className="h-10" />
              </div>
            ) : list!.length === 0 ? (
              <EmptyState title="No members yet" />
            ) : (
              <ul className="divide-line divide-y">
                {newestMembers(list!, 5).map((u) => (
                  <li key={u.id}>
                    <Link
                      to={`/admin/members/${u.id}`}
                      className="hover:bg-raised/50 -mx-2 flex items-center gap-3 rounded-md px-2 py-2.5"
                    >
                      <span className="min-w-0 flex-1 truncate font-medium">
                        {primaryRsn(u) || u.discordAccount.username}
                      </span>
                      <RankBadge rank={u.rank} size="sm" />
                      <span className="text-muted hidden w-28 text-right text-sm sm:inline">
                        {formatDate(u.dateCreated)}
                      </span>
                    </Link>
                  </li>
                ))}
              </ul>
            )}
          </Card>
        </div>
      )}
    </>
  )
}
