import { Search, UserPlus } from 'lucide-react'
import { useState, type KeyboardEvent } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router'
import { useUsers, useUsersByHistoricalRsn } from '@/api/hooks/users'
import { POINT_RANKS, ROLE_RANKS, type AppRole, type GetUserResponse, type Rank } from '@/api/types'
import { RoleBadge } from '@/components/admin/RoleBadge'
import {
  filterMembers,
  parseMemberParams,
  serializeMemberParams,
  sortMembers,
  type MemberFilters,
  type MemberSort,
  type SortKey,
  type StatusFilter,
} from '@/components/admin/memberFilters'
import {
  Badge,
  EmptyState,
  ErrorState,
  Input,
  PageHeader,
  RankBadge,
  Select,
  Skeleton,
  Table,
  TableBody,
  TableHead,
  Td,
  Th,
  Tr,
  buttonClass,
} from '@/components/ui'
import { formatNumber } from '@/lib/format'
import { rankLabel, roleLabel } from '@/lib/ranks'

const ROLES: AppRole[] = ['Member', 'Administrator', 'SuperAdministrator', 'Owner']

function MemberRow({ user, onOpen }: { user: GetUserResponse; onOpen: () => void }) {
  const rsns = user.runescapeAccounts.map((a) => a.username)
  return (
    <Tr
      className="cursor-pointer"
      onClick={onOpen}
      onKeyDown={(e: KeyboardEvent) => {
        if (e.key === 'Enter' && e.target === e.currentTarget) onOpen()
      }}
      tabIndex={0}
    >
      <Td className="font-medium">
        {rsns.length ? (
          <Link
            to={`/admin/members/${user.id}`}
            onClick={(e) => e.stopPropagation()}
            className="hover:text-primary"
          >
            {rsns[0]}
          </Link>
        ) : (
          <span className="text-muted">No RSN</span>
        )}
        {rsns.length > 1 && (
          <span className="text-muted ml-2 text-xs" title={rsns.slice(1).join(', ')}>
            +{rsns.length - 1} more
          </span>
        )}
      </Td>
      <Td>{user.discordAccount.username}</Td>
      <Td>
        <RankBadge rank={user.rank} size="sm" />
      </Td>
      <Td align="right" numeric>
        {formatNumber(user.clanPoints)}
      </Td>
      <Td>
        <RoleBadge role={user.appRole} />
      </Td>
      <Td>
        <Badge tone={user.isActive ? 'success' : 'danger'}>
          {user.isActive ? 'Active' : 'Inactive'}
        </Badge>
      </Td>
    </Tr>
  )
}

function MembersTable({
  members,
  sort,
  onSort,
}: {
  members: GetUserResponse[]
  sort?: MemberSort
  onSort?: (key: SortKey) => void
}) {
  const navigate = useNavigate()
  const header = (key: SortKey, label: string, align?: 'right') => (
    <Th
      align={align}
      onSort={onSort && (() => onSort(key))}
      sortDirection={sort?.key === key ? sort.dir : null}
    >
      {label}
    </Th>
  )
  return (
    <Table>
      <TableHead>
        <tr>
          {header('rsn', 'RSN')}
          {header('discord', 'Discord')}
          {header('rank', 'Rank')}
          {header('points', 'Points', 'right')}
          {header('role', 'Role')}
          {header('status', 'Status')}
        </tr>
      </TableHead>
      <TableBody>
        {members.map((u) => (
          <MemberRow key={u.id} user={u} onOpen={() => navigate(`/admin/members/${u.id}`)} />
        ))}
      </TableBody>
    </Table>
  )
}

export function Component() {
  const [params, setParams] = useSearchParams()
  const { filters, sort } = parseMemberParams(params)
  const users = useUsers()
  // The term the user confirmed with Enter after finding no local match.
  const [historicalTerm, setHistoricalTerm] = useState<string | undefined>()
  const historical = useUsersByHistoricalRsn(historicalTerm)

  const update = (nextFilters: MemberFilters, nextSort: MemberSort = sort) =>
    setParams(serializeMemberParams(nextFilters, nextSort), { replace: true })

  const setFilter = <K extends keyof MemberFilters>(key: K, value: MemberFilters[K]) => {
    if (key === 'q') setHistoricalTerm(undefined)
    update({ ...filters, [key]: value })
  }

  const toggleSort = (key: SortKey) =>
    update(filters, {
      key,
      dir:
        sort.key === key && sort.dir === 'asc'
          ? 'desc'
          : sort.key === key
            ? 'asc'
            : key === 'points'
              ? 'desc'
              : 'asc',
    })

  const all = users.data?.users ?? []
  const visible = sortMembers(filterMembers(all, filters), sort)
  const hasFilters = !!(filters.q || filters.rank || filters.role || filters.status !== 'all')
  const term = filters.q.trim()

  const onSearchKey = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && term && users.isSuccess && visible.length === 0) {
      setHistoricalTerm(term)
    }
  }

  const historicalMatches =
    historicalTerm && historicalTerm === term ? (historical.data?.users ?? []) : []
  const showHistorical = !!historicalTerm && historicalTerm === term && visible.length === 0

  let body
  if (users.isPending) {
    body = (
      <div className="space-y-2" aria-busy="true">
        {Array.from({ length: 8 }, (_, i) => (
          <Skeleton key={i} className="h-12" />
        ))}
      </div>
    )
  } else if (users.isError) {
    body = (
      <ErrorState
        error={users.error}
        title="Couldn't load members"
        onRetry={() => users.refetch()}
      />
    )
  } else if (visible.length > 0) {
    body = <MembersTable members={visible} sort={sort} onSort={toggleSort} />
  } else if (showHistorical) {
    body = historical.isPending ? (
      <Skeleton className="h-24" />
    ) : historical.isError ? (
      <ErrorState
        error={historical.error}
        title="Couldn't search previous RSNs"
        onRetry={() => historical.refetch()}
      />
    ) : historicalMatches.length > 0 ? (
      <div className="space-y-3">
        <p className="text-muted text-sm">
          No current match for &ldquo;{term}&rdquo;, but{' '}
          {historicalMatches.length === 1 ? 'this member' : 'these members'} used it as a{' '}
          <strong className="text-fg">previous RSN</strong>:
        </p>
        <MembersTable members={historicalMatches} />
      </div>
    ) : (
      <EmptyState
        icon={<Search aria-hidden />}
        title="No matches"
        description={`Nobody has used “${term}” as a current or previous RSN.`}
      />
    )
  } else {
    body = (
      <EmptyState
        icon={<Search aria-hidden />}
        title={hasFilters ? 'No members match your filters' : 'No members yet'}
        description={
          term
            ? 'Press Enter to also search previous RSNs.'
            : hasFilters
              ? 'Try loosening or clearing a filter.'
              : undefined
        }
        action={
          hasFilters ? (
            <button
              type="button"
              className={buttonClass('secondary', 'sm')}
              onClick={() => {
                setHistoricalTerm(undefined)
                setParams(new URLSearchParams(), { replace: true })
              }}
            >
              Clear filters
            </button>
          ) : undefined
        }
      />
    )
  }

  return (
    <>
      <PageHeader
        title="Members"
        description={
          users.isSuccess
            ? `${formatNumber(visible.length)} of ${formatNumber(all.length)} members`
            : undefined
        }
        actions={
          <Link to="/admin/members/new" className={buttonClass('primary', 'md')}>
            <UserPlus aria-hidden className="size-4" />
            Add member
          </Link>
        }
      />

      <div className="mb-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-[2fr_1fr_1fr_1fr]">
        <div className="relative sm:col-span-2 lg:col-span-1">
          <Search
            aria-hidden
            className="text-muted pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2"
          />
          <Input
            type="search"
            value={filters.q}
            onChange={(e) => setFilter('q', e.target.value)}
            onKeyDown={onSearchKey}
            placeholder="Search RSN or Discord name"
            aria-label="Search members"
            className="pl-9"
          />
        </div>
        <Select
          aria-label="Filter by rank"
          value={filters.rank}
          onChange={(e) => setFilter('rank', e.target.value as Rank | '')}
        >
          <option value="">All ranks</option>
          {[...POINT_RANKS, ...ROLE_RANKS].map((r) => (
            <option key={r} value={r}>
              {rankLabel(r)}
            </option>
          ))}
        </Select>
        <Select
          aria-label="Filter by role"
          value={filters.role}
          onChange={(e) => setFilter('role', e.target.value as AppRole | '')}
        >
          <option value="">All roles</option>
          {ROLES.map((r) => (
            <option key={r} value={r}>
              {roleLabel(r)}
            </option>
          ))}
        </Select>
        <Select
          aria-label="Filter by status"
          value={filters.status}
          onChange={(e) => setFilter('status', e.target.value as StatusFilter)}
        >
          <option value="all">Active &amp; inactive</option>
          <option value="active">Active only</option>
          <option value="inactive">Inactive only</option>
        </Select>
      </div>

      {body}
    </>
  )
}
