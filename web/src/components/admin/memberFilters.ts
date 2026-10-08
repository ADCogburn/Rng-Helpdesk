import type { AppRole, GetUserResponse, Rank } from '@/api/types'
import { POINT_RANKS, ROLE_RANKS } from '@/api/types'

export type SortKey = 'rsn' | 'discord' | 'rank' | 'points' | 'role' | 'status'
export type SortDir = 'asc' | 'desc'
export type StatusFilter = 'all' | 'active' | 'inactive'

export interface MemberFilters {
  q: string
  rank: Rank | ''
  role: AppRole | ''
  status: StatusFilter
}

export interface MemberSort {
  key: SortKey
  dir: SortDir
}

export const DEFAULT_SORT: MemberSort = { key: 'points', dir: 'desc' }

const ALL_RANKS: readonly string[] = [...POINT_RANKS, ...ROLE_RANKS]
const ROLES: readonly AppRole[] = ['Member', 'Administrator', 'SuperAdministrator', 'Owner']
const SORT_KEYS: readonly SortKey[] = ['rsn', 'discord', 'rank', 'points', 'role', 'status']

/** Ascending position of a rank on the ladder (point ranks first, then role ranks). */
export const rankOrder = (rank: Rank) => ALL_RANKS.indexOf(rank)
export const roleOrder = (role: AppRole) => ROLES.indexOf(role)

export const primaryRsn = (user: GetUserResponse) => user.runescapeAccounts[0]?.username ?? ''

/** Parses filter + sort state from URL params, ignoring anything unrecognised. */
export function parseMemberParams(params: URLSearchParams): {
  filters: MemberFilters
  sort: MemberSort
} {
  const rank = params.get('rank') ?? ''
  const role = params.get('role') ?? ''
  const status = params.get('status')
  const key = params.get('sort')
  const dir = params.get('dir')
  const sortKey = SORT_KEYS.find((k) => k === key)
  return {
    filters: {
      q: params.get('q') ?? '',
      rank: ALL_RANKS.includes(rank) ? (rank as Rank) : '',
      role: ROLES.find((r) => r === role) ?? '',
      status: status === 'active' || status === 'inactive' ? status : 'all',
    },
    sort: sortKey ? { key: sortKey, dir: dir === 'asc' ? 'asc' : 'desc' } : DEFAULT_SORT,
  }
}

/** Inverse of `parseMemberParams`; defaults are omitted to keep URLs short. */
export function serializeMemberParams(filters: MemberFilters, sort: MemberSort): URLSearchParams {
  const params = new URLSearchParams()
  if (filters.q) params.set('q', filters.q)
  if (filters.rank) params.set('rank', filters.rank)
  if (filters.role) params.set('role', filters.role)
  if (filters.status !== 'all') params.set('status', filters.status)
  if (sort.key !== DEFAULT_SORT.key || sort.dir !== DEFAULT_SORT.dir) {
    params.set('sort', sort.key)
    params.set('dir', sort.dir)
  }
  return params
}

/** Case-insensitive match on any RSN, the Discord name, or the exact id. */
export function matchesQuery(user: GetUserResponse, q: string): boolean {
  const needle = q.trim().toLowerCase()
  if (!needle) return true
  return (
    user.runescapeAccounts.some((a) => a.username.toLowerCase().includes(needle)) ||
    user.discordAccount.username.toLowerCase().includes(needle) ||
    user.id === needle
  )
}

export function filterMembers(users: GetUserResponse[], filters: MemberFilters): GetUserResponse[] {
  return users.filter(
    (u) =>
      matchesQuery(u, filters.q) &&
      (!filters.rank || u.rank === filters.rank) &&
      (!filters.role || u.appRole === filters.role) &&
      (filters.status === 'all' || u.isActive === (filters.status === 'active')),
  )
}

/** Alphabetical by primary RSN; members without an RSN always come last. */
function compareRsn(a: GetUserResponse, b: GetUserResponse): number {
  const ra = primaryRsn(a)
  const rb = primaryRsn(b)
  if (!ra !== !rb) return ra ? -1 : 1
  return ra.localeCompare(rb, undefined, { sensitivity: 'base' })
}

function compareBy(key: Exclude<SortKey, 'rsn'>, a: GetUserResponse, b: GetUserResponse): number {
  switch (key) {
    case 'discord':
      return a.discordAccount.username.localeCompare(b.discordAccount.username, undefined, {
        sensitivity: 'base',
      })
    case 'rank':
      return rankOrder(a.rank) - rankOrder(b.rank)
    case 'points':
      return a.clanPoints - b.clanPoints
    case 'role':
      return roleOrder(a.appRole) - roleOrder(b.appRole)
    case 'status':
      return Number(a.isActive) - Number(b.isActive)
  }
}

/** Stable sort; ties fall back to RSN so the order is deterministic. Members without an RSN sort last for `rsn`. */
export function sortMembers(users: GetUserResponse[], sort: MemberSort): GetUserResponse[] {
  const factor = sort.dir === 'asc' ? 1 : -1
  return [...users].sort((a, b) => {
    if (sort.key === 'rsn') {
      const aEmpty = !primaryRsn(a)
      const bEmpty = !primaryRsn(b)
      if (aEmpty !== bEmpty) return aEmpty ? 1 : -1
      return compareRsn(a, b) * factor
    }
    return compareBy(sort.key, a, b) * factor || compareRsn(a, b)
  })
}
export interface RankCount {
  rank: Rank
  count: number
}

/** Counts active members per rank, ladder order, omitting ranks nobody holds. */
export function rankDistribution(users: GetUserResponse[]): RankCount[] {
  const counts = new Map<Rank, number>()
  for (const u of users) if (u.isActive) counts.set(u.rank, (counts.get(u.rank) ?? 0) + 1)
  return [...counts.entries()]
    .map(([rank, count]) => ({ rank, count }))
    .sort((a, b) => rankOrder(a.rank) - rankOrder(b.rank))
}

export interface DashboardStats {
  active: number
  inactive: number
  admins: number
  totalPoints: number
}

export function dashboardStats(users: GetUserResponse[]): DashboardStats {
  let active = 0
  let admins = 0
  let totalPoints = 0
  for (const u of users) {
    if (!u.isActive) continue
    active++
    totalPoints += u.clanPoints
    if (u.appRole !== 'Member') admins++
  }
  return { active, inactive: users.length - active, admins, totalPoints }
}

export function newestMembers(users: GetUserResponse[], count: number): GetUserResponse[] {
  return [...users]
    .sort((a, b) => Date.parse(b.dateCreated) - Date.parse(a.dateCreated))
    .slice(0, count)
}
