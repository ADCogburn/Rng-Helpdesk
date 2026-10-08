import { useQuery } from '@tanstack/react-query'
import { api } from '../client'
import { queryKeys } from '../queryKeys'
import type {
  PublicLeaderboardResponse,
  PublicOverviewResponse,
  PublicRanksResponse,
} from '../types'

export const usePublicOverview = () =>
  useQuery({
    queryKey: queryKeys.public.overview,
    queryFn: () => api.get<PublicOverviewResponse>('/public/overview'),
    staleTime: 60_000,
  })

export const usePublicRanks = () =>
  useQuery({
    queryKey: queryKeys.public.ranks,
    queryFn: () => api.get<PublicRanksResponse>('/public/ranks'),
    staleTime: 5 * 60_000,
  })

export const usePublicLeaderboard = (top = 25) =>
  useQuery({
    queryKey: queryKeys.public.leaderboard(top),
    queryFn: () => api.get<PublicLeaderboardResponse>('/public/leaderboard', { query: { top } }),
    staleTime: 60_000,
  })
