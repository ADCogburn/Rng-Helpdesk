// Mirrors the API contract in docs/ui/PLAN.md. Discord/user ids are strings on the wire
// (they exceed Number.MAX_SAFE_INTEGER), enums serialize as strings.

export type AppRole = 'Member' | 'Administrator' | 'SuperAdministrator' | 'Owner'

export const POINT_RANKS = [
  'Bronze',
  'Iron',
  'Steel',
  'Mithril',
  'Adamant',
  'Rune',
  'Dragon',
  'Sapphire',
  'Emerald',
  'Ruby',
  'Diamond',
  'Dragonstone',
  'Onyx',
  'Zenyte',
] as const
export const ROLE_RANKS = ['Administrator', 'DeputyOwner', 'Owner'] as const

export type PointRank = (typeof POINT_RANKS)[number]
export type RoleRank = (typeof ROLE_RANKS)[number]
export type Rank = PointRank | RoleRank

export interface DiscordAccount {
  discordId: string
  username: string
}

export interface RunescapeAccount {
  username: string
}

// --- auth ---
export interface LoginRequest {
  username: string
  password: string
}
export interface LoginResponse {
  token: string
  mustChangePassword: boolean
}
export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

// --- users ---
export interface GetUserResponse {
  id: string
  appRole: AppRole
  clanPoints: number
  rank: Rank
  isActive: boolean
  dateCreated: string
  discordAccount: DiscordAccount
  runescapeAccounts: RunescapeAccount[]
}
export interface GetUsersResponse {
  totalCount: number
  users: GetUserResponse[]
}
export interface GetUsersByHistoricalRsnResponse {
  users: GetUserResponse[]
}

export interface LifecycleHistoryItem {
  action: string
  occurredAt: string
}
export interface UserLifecycleResponse {
  userId: string
  history: LifecycleHistoryItem[]
}

export interface AdjustPointsRequest {
  points: number
  reason: string
}
export interface PointHistoryEvent {
  delta: number
  reason: string
  occurredAt: string
  rankBefore?: Rank | null
  rankAfter?: Rank | null
}
export interface PointHistoryResponse {
  userId: string
  totalEventCount: number
  events: PointHistoryEvent[]
}

export interface RunescapeAccountsResponse {
  accounts: RunescapeAccount[]
}
export interface RunescapeAccountHistoryItem {
  changeType: string
  username?: string | null
  oldUsername?: string | null
  newUsername?: string | null
  occurredAt: string
}
export interface RunescapeAccountHistoryResponse {
  history: RunescapeAccountHistoryItem[]
}
export interface RunescapeUsernameRequest {
  username: string
}
export interface RenameRunescapeAccountRequest {
  oldUsername: string
  newUsername: string
}

// --- admin ---
export interface CreateUserRequest {
  discordAccount: DiscordAccount
  runescapeAccounts: RunescapeAccount[]
}
export interface CreateUserResponse {
  userId: string
  username: string
  temporaryPassword: string
}

// --- rank thresholds ---
export interface RankThreshold {
  rank: PointRank
  pointsRequired: number
}
export interface RankThresholdsResponse {
  thresholds: RankThreshold[]
}
export interface UpdateRankThresholdRequest {
  pointsRequired: number
}

// --- public ---
export interface RankDistributionItem {
  rank: PointRank
  count: number
}
export interface PublicOverviewResponse {
  activeMemberCount: number
  totalClanPoints: number
  rankDistribution: RankDistributionItem[]
}
export interface PublicRanksResponse {
  ranks: RankThreshold[]
}
export interface LeaderboardEntry {
  position: number
  runescapeUsername: string
  rank: Rank
  clanPoints: number
}
export interface PublicLeaderboardResponse {
  entries: LeaderboardEntry[]
}
