import { POINT_RANKS, type AppRole, type Rank } from '@/api/types'

/** Rank colour tokens; each is defined as `--color-rank-<name>` in index.css. */
export const RANK_COLORS: Record<Rank, string> = {
  Bronze: '#a8703a',
  Iron: '#8a8d91',
  Steel: '#b4bcc6',
  Mithril: '#5a64b8',
  Adamant: '#4f8a5b',
  Rune: '#4fb3c7',
  Dragon: '#c0392b',
  Sapphire: '#2f6fd6',
  Emerald: '#2ea86b',
  Ruby: '#c21f4a',
  Diamond: '#cfe8f5',
  Dragonstone: '#a35bd6',
  Onyx: '#2b2b2b',
  Zenyte: '#f0a33a',
  Administrator: '#d4a84b',
  DeputyOwner: '#d4a84b',
  Owner: '#d4a84b',
}

export function rankColorVar(rank: Rank): string {
  return `var(--color-rank-${rank.toLowerCase()})`
}

export function isPointRank(rank: Rank): boolean {
  return (POINT_RANKS as readonly string[]).includes(rank)
}

export type RankIconKind = 'crown' | 'shield' | 'gem'

export function rankIconKind(rank: Rank): RankIconKind {
  if (rank === 'Owner' || rank === 'DeputyOwner') return 'crown'
  if (rank === 'Administrator') return 'shield'
  return 'gem'
}

/** "DeputyOwner" -> "Deputy Owner" */
export function rankLabel(rank: Rank): string {
  return rank.replace(/([a-z])([A-Z])/g, '$1 $2')
}

export const ADMIN_ROLES: readonly AppRole[] = ['Administrator', 'SuperAdministrator', 'Owner']

export function isAdminRole(role: AppRole | undefined | null): boolean {
  return !!role && ADMIN_ROLES.includes(role)
}

export function roleLabel(role: AppRole): string {
  return role.replace(/([a-z])([A-Z])/g, '$1 $2')
}
