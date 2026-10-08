import { describe, expect, it } from 'vitest'
import type { PointRank } from '@/api/types'
import {
  canSaveThreshold,
  neighboursOf,
  parseThresholdDraft,
  validateThreshold,
} from './rankThresholds'

const saved = new Map<PointRank, number>([
  ['Bronze', 0],
  ['Iron', 100],
  ['Steel', 250],
  ['Mithril', 500],
  ['Zenyte', 90000],
])

describe('neighboursOf', () => {
  it('returns the saved neighbours on either side of a rank', () => {
    expect(neighboursOf('Steel', saved)).toEqual({
      previous: { rank: 'Iron', pointsRequired: 100 },
      next: { rank: 'Mithril', pointsRequired: 500 },
    })
  })

  it('has no previous rank for Bronze and no next rank for Zenyte', () => {
    expect(neighboursOf('Bronze', saved).previous).toBeUndefined()
    expect(neighboursOf('Zenyte', saved).next).toBeUndefined()
  })

  it('skips neighbours the API did not return', () => {
    expect(neighboursOf('Mithril', saved)).toEqual({
      previous: { rank: 'Steel', pointsRequired: 250 },
      next: undefined,
    })
    expect(neighboursOf('Onyx', saved)).toEqual({
      previous: undefined,
      next: { rank: 'Zenyte', pointsRequired: 90000 },
    })
  })
})

describe('parseThresholdDraft', () => {
  it('accepts non-negative whole numbers, ignoring surrounding spaces', () => {
    expect(parseThresholdDraft('0')).toBe(0)
    expect(parseThresholdDraft(' 1500 ')).toBe(1500)
  })

  it('rejects blanks, negatives, decimals and non-numeric text', () => {
    for (const draft of ['', '  ', '-1', '1.5', '1e3', 'abc', '12px'])
      expect(parseThresholdDraft(draft)).toBeNull()
  })

  it('rejects numbers beyond the safe integer range', () => {
    expect(parseThresholdDraft('9007199254740993')).toBeNull()
  })
})

describe('validateThreshold', () => {
  const steel = neighboursOf('Steel', saved)

  it('requires a whole number', () => {
    expect(validateThreshold('', steel)).toBe('Enter a whole number, 0 or more.')
    expect(validateThreshold('-5', steel)).toBe('Enter a whole number, 0 or more.')
  })

  it('must be strictly above the previous rank', () => {
    expect(validateThreshold('100', steel)).toBe('Must be more than Iron (100).')
    expect(validateThreshold('99', steel)).toBe('Must be more than Iron (100).')
    expect(validateThreshold('101', steel)).toBeNull()
  })

  it('must be strictly below the next rank', () => {
    expect(validateThreshold('500', steel)).toBe('Must be less than Mithril (500).')
    expect(validateThreshold('499', steel)).toBeNull()
  })

  it('lets Bronze go as low as zero but keeps it below Iron', () => {
    const bronze = neighboursOf('Bronze', saved)
    expect(validateThreshold('0', bronze)).toBeNull()
    expect(validateThreshold('100', bronze)).toBe('Must be less than Iron (100).')
  })

  it('formats large bounds with separators', () => {
    const zenyte = neighboursOf('Zenyte', new Map([['Onyx', 75000]]))
    expect(validateThreshold('75000', zenyte)).toBe('Must be more than Onyx (75,000).')
  })
})

describe('canSaveThreshold', () => {
  const steel = neighboursOf('Steel', saved)

  it('allows a changed, valid value', () => {
    expect(canSaveThreshold('300', 250, steel)).toBe(true)
  })

  it('does not allow an unchanged value', () => {
    expect(canSaveThreshold('250', 250, steel)).toBe(false)
    expect(canSaveThreshold(' 250 ', 250, steel)).toBe(false)
  })

  it('does not allow an invalid value', () => {
    expect(canSaveThreshold('', 250, steel)).toBe(false)
    expect(canSaveThreshold('100', 250, steel)).toBe(false)
    expect(canSaveThreshold('500', 250, steel)).toBe(false)
  })

  it('allows saving a rank the API has not returned yet', () => {
    expect(canSaveThreshold('42000', undefined, neighboursOf('Onyx', saved))).toBe(true)
  })
})
