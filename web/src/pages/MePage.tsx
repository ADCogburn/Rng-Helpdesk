import { useAuth } from '@/auth'
import { RouteFallback } from '@/components/layout/RouteFallback'
import {
  AccountActions,
  ProfileHeader,
  RankProgressCard,
  RunescapeAccountsCard,
} from '@/components/me'
import { PageHeader } from '@/components/ui'

export function Component() {
  const { user } = useAuth()
  if (!user) return <RouteFallback />

  return (
    <main className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-10 sm:px-6 sm:py-14">
      <PageHeader title="My profile" description="Your standing in the clan at a glance." />
      <ProfileHeader user={user} />
      <div className="grid gap-6 lg:grid-cols-2">
        <RankProgressCard clanPoints={user.clanPoints} rank={user.rank} />
        <RunescapeAccountsCard accounts={user.runescapeAccounts} />
      </div>
      <AccountActions />
    </main>
  )
}
