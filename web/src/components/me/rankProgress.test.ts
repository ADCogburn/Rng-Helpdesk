import { describe, expect, it } from 'vitest'
import type { RankThreshold } from '@/api/types'
import { rankProgress } from './rankProgress'

const thresholds: RankThreshold[] = [
  { rank: 'Bronze', pointsRequired: 0 },
  { rank: 'Iron', pointsRequired: 100 },
  { rank: 'Steel', pointsRequired: 300 },
  { rank: 'Zenyte', pointsRequired: 1000 },
]

describe('rankProgress', () => {
  it('starts at 0% when the user has just reached their rank', () => {
    expect(rankProgress(100, 'Iron', thresholds)).toEqual({
      status: 'next',
      nextRank: 'Steel',
      fraction: 0,
      pointsToNext: 200,
    })
  })

  it('reports the fraction of the band and the points still needed', () => {
    expect(rankProgress(200, 'Iron', thresholds)).toEqual({
      status: 'next',
      nextRank: 'Steel',
      fraction: 0.5,
      pointsToNext: 100,
    })
  })

  it('reports the next rank from the lowest tier', () => {
    const result = rankProgress(25, 'Bronze', thresholds)
    expect(result).toEqual({ status: 'next', nextRank: 'Iron', fraction: 0.25, pointsToNext: 75 })
  })

  it('treats the top point-based rank as max', () => {
    expect(rankProgress(5000, 'Zenyte', thresholds)).toEqual({ status: 'max' })
  })

  it('treats admin-tier ranks as role-based, not points-based', () => {
    expect(rankProgress(5000, 'Owner', thresholds)).toEqual({ status: 'role' })
    expect(rankProgress(0, 'Administrator', thresholds)).toEqual({ status: 'role' })
    expect(rankProgress(0, 'DeputyOwner', thresholds)).toEqual({ status: 'role' })
  })

  it('reports unknown when the rank has no threshold', () => {
    expect(rankProgress(50, 'Diamond', thresholds)).toEqual({ status: 'unknown' })
  })

  it('clamps progress when points sit below the threshold', () => {
    expect(rankProgress(90, 'Iron', thresholds)).toEqual({
      status: 'next',
      nextRank: 'Steel',
      fraction: 0,
      pointsToNext: 210,
    })
  })
})
