import { describe, expect, it } from 'vitest'
import {
  emptyNewMemberDraft,
  hasErrors,
  toCreateUserRequest,
  validateNewMember,
  type NewMemberDraft,
} from './validation'

const valid: NewMemberDraft = {
  discordId: '123456789012345678',
  discordUsername: 'wiz',
  rsns: ['Zezima', 'Old Zez'],
}

describe('validateNewMember', () => {
  it('accepts a complete draft', () => {
    expect(hasErrors(validateNewMember(valid))).toBe(false)
  })

  it('accepts a draft with no RuneScape names', () => {
    expect(hasErrors(validateNewMember({ ...valid, rsns: [] }))).toBe(false)
  })

  it('requires the Discord ID and username', () => {
    const errors = validateNewMember(emptyNewMemberDraft)
    expect(errors.discordId).toBe('Discord ID is required.')
    expect(errors.discordUsername).toBe('Discord username is required.')
  })

  it('accepts 17 to 20 digits and rejects anything else', () => {
    for (const id of ['1'.repeat(17), '9'.repeat(20)]) {
      expect(validateNewMember({ ...valid, discordId: id }).discordId).toBeUndefined()
    }
    for (const id of ['1'.repeat(16), '1'.repeat(21), '12345abc901234567', '1234.5678901234567']) {
      expect(validateNewMember({ ...valid, discordId: id }).discordId).toBeDefined()
    }
  })

  it('checks RuneScape names against the API rule per row', () => {
    const errors = validateNewMember({
      ...valid,
      rsns: ['Good', 'thirteenchars', 'bad_name', 'ok-name 1'],
    })
    expect(Object.keys(errors.rsns).map(Number)).toEqual([1, 2])
    expect(errors.rsns[3]).toBeUndefined()
  })

  it('trims surrounding spaces before checking a name', () => {
    expect(hasErrors(validateNewMember({ ...valid, rsns: [' lead', 'trail '] }))).toBe(false)
  })

  it('accepts names at the length and character limits', () => {
    const errors = validateNewMember({ ...valid, rsns: ['a', 'Twelve Chars', 'a-b c-d'] })
    expect(errors.rsns).toEqual({})
  })

  it('flags a repeated name case-insensitively on the later row only', () => {
    const errors = validateNewMember({ ...valid, rsns: ['Zezima', 'zezima'] })
    expect(errors.rsns).toEqual({ 1: 'This name is already listed above.' })
  })

  it('ignores blank rows', () => {
    expect(hasErrors(validateNewMember({ ...valid, rsns: ['', '   ', 'Zezima'] }))).toBe(false)
  })
})

describe('toCreateUserRequest', () => {
  it('keeps the Discord ID as a string and trims values', () => {
    const body = toCreateUserRequest({
      discordId: ' 123456789012345678 ',
      discordUsername: ' wiz ',
      rsns: [' Zezima ', '', '  ', 'Old Zez'],
    })
    expect(body).toEqual({
      discordAccount: { discordId: '123456789012345678', username: 'wiz' },
      runescapeAccounts: [{ username: 'Zezima' }, { username: 'Old Zez' }],
    })
    expect(typeof body.discordAccount.discordId).toBe('string')
  })
})
