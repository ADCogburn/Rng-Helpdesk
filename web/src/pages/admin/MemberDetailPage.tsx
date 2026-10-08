import { Gem, History, ListChecks, UserRound } from 'lucide-react'
import { Link, useParams, useSearchParams } from 'react-router'
import { useUser } from '@/api/hooks/users'
import { ApiError } from '@/api/errors'
import { useAuth } from '@/auth'
import {
  LifecycleTab,
  MemberHeader,
  OverviewTab,
  PointsTab,
  RunescapeTab,
} from '@/components/admin/member'
import {
  buttonClass,
  EmptyState,
  ErrorState,
  PageHeader,
  Skeleton,
  TabPanel,
  Tabs,
  type TabItem,
} from '@/components/ui'

const TABS: TabItem[] = [
  { id: 'overview', label: 'Overview', icon: <UserRound aria-hidden className="size-4" /> },
  { id: 'accounts', label: 'RuneScape accounts', icon: <Gem aria-hidden className="size-4" /> },
  { id: 'points', label: 'Points', icon: <ListChecks aria-hidden className="size-4" /> },
  { id: 'lifecycle', label: 'Lifecycle', icon: <History aria-hidden className="size-4" /> },
]

export function Component() {
  const { id } = useParams()
  const [params, setParams] = useSearchParams()
  const { user: viewer } = useAuth()
  const q = useUser(id)

  const requested = params.get('tab') ?? 'overview'
  const tab = TABS.some((t) => t.id === requested) ? requested : 'overview'

  if (q.isPending) {
    return (
      <div aria-busy="true" className="space-y-4">
        <Skeleton className="h-32" />
        <Skeleton className="h-64" />
      </div>
    )
  }

  if (q.isError) {
    if (q.error instanceof ApiError && q.error.status === 404) {
      return (
        <>
          <PageHeader title="Member not found" />
          <EmptyState
            title="No such member"
            description="This member doesn't exist, or the link is wrong."
            action={
              <Link to="/admin/members" className={buttonClass('primary', 'md')}>
                Back to Members
              </Link>
            }
          />
        </>
      )
    }
    return <ErrorState error={q.error} title="Couldn't load member" onRetry={() => q.refetch()} />
  }

  const user = q.data
  return (
    <>
      <Link to="/admin/members" className="text-muted hover:text-primary mb-3 inline-block text-sm">
        &larr; Members
      </Link>
      <MemberHeader user={user} />
      <Tabs
        tabs={TABS}
        value={tab}
        label="Member sections"
        onChange={(next) => setParams(next === 'overview' ? {} : { tab: next }, { replace: true })}
      />
      <TabPanel id="overview" value={tab}>
        <OverviewTab user={user} viewerId={viewer?.id} />
      </TabPanel>
      <TabPanel id="accounts" value={tab}>
        <RunescapeTab id={user.id} />
      </TabPanel>
      <TabPanel id="points" value={tab}>
        <PointsTab id={user.id} />
      </TabPanel>
      <TabPanel id="lifecycle" value={tab}>
        <LifecycleTab id={user.id} />
      </TabPanel>
    </>
  )
}
