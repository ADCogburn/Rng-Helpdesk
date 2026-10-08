import type { CreateUserRequest } from '@/api/types'

/** Discord snowflakes are 17–20 digits. Kept as a string everywhere; never converted to a number. */
export const DISCORD_ID_PATTERN = /^\d{17,20}$/
/** Same rule the API enforces for RuneScape names (docs/ui/PLAN.md). */
export const RSN_PATTERN = /^(?! )[A-Za-z0-9 -]{1,12}(?<! )$/

export interface NewMemberDraft {
  discordId: string
  discordUsername: string
  /** Raw RuneScape name inputs. Blank rows are ignored. */
  rsns: string[]
}

export interface NewMemberErrors {
  discordId?: string
  discordUsername?: string
  /** Keyed by row index into `draft.rsns`. */
  rsns: Record<number, string>
}

export const emptyNewMemberDraft: NewMemberDraft = { discordId: '', discordUsername: '', rsns: [] }

export function validateNewMember(draft: NewMemberDraft): NewMemberErrors {
  const errors: NewMemberErrors = { rsns: {} }

  const discordId = draft.discordId.trim()
  if (!discordId) errors.discordId = 'Discord ID is required.'
  else if (!DISCORD_ID_PATTERN.test(discordId)) errors.discordId = 'A Discord ID is 17–20 digits.'

  if (!draft.discordUsername.trim()) errors.discordUsername = 'Discord username is required.'

  // RuneScape names are case-insensitive, so duplicates are compared lowercased.
  const seen = new Set<string>()
  draft.rsns.forEach((raw, i) => {
    const rsn = raw.trim()
    if (!rsn) return
    if (!RSN_PATTERN.test(rsn)) {
      errors.rsns[i] =
        'Use 1–12 letters, numbers, spaces or hyphens, with no leading or trailing space.'
      return
    }
    const key = rsn.toLowerCase()
    if (seen.has(key)) errors.rsns[i] = 'This name is already listed above.'
    else seen.add(key)
  })

  return errors
}

export function hasErrors(errors: NewMemberErrors): boolean {
  return !!(errors.discordId || errors.discordUsername || Object.keys(errors.rsns).length > 0)
}

/** Builds the POST /admin/create body from a draft that has already passed validation. */
export function toCreateUserRequest(draft: NewMemberDraft): CreateUserRequest {
  return {
    discordAccount: {
      discordId: draft.discordId.trim(),
      username: draft.discordUsername.trim(),
    },
    runescapeAccounts: draft.rsns
      .map((rsn) => rsn.trim())
      .filter((rsn) => rsn.length > 0)
      .map((username) => ({ username })),
  }
}
