import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../client'
import { queryKeys } from '../queryKeys'
import type {
  CreateUserRequest,
  CreateUserResponse,
  PointRank,
  RankThresholdsResponse,
  UpdateRankThresholdRequest,
} from '../types'

const enc = encodeURIComponent

export function useCreateUser() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: CreateUserRequest) => api.post<CreateUserResponse>('/admin/create', body),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: queryKeys.users.list })
      void qc.invalidateQueries({ queryKey: ['public'] })
    },
  })
}

export type AdminAction = 'promote' | 'demote' | 'deactivate' | 'reactivate'

/** promote / demote / deactivate / reactivate a user by id. */
export function useAdminAction(action: AdminAction) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => api.post(`/admin/${enc(id)}/${action}`),
    onSuccess: (_data, id) => {
      void qc.invalidateQueries({ queryKey: queryKeys.users.detail(id) })
      void qc.invalidateQueries({ queryKey: ['users', id] })
      void qc.invalidateQueries({ queryKey: queryKeys.users.list })
      void qc.invalidateQueries({ queryKey: ['public'] })
    },
  })
}

export const usePromoteUser = () => useAdminAction('promote')
export const useDemoteUser = () => useAdminAction('demote')
export const useDeactivateUser = () => useAdminAction('deactivate')
export const useReactivateUser = () => useAdminAction('reactivate')

export const useRankThresholds = () =>
  useQuery({
    queryKey: queryKeys.rankThresholds,
    queryFn: () => api.get<RankThresholdsResponse>('/rankthresholds'),
  })

export function useUpdateRankThreshold() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ rank, pointsRequired }: { rank: PointRank } & UpdateRankThresholdRequest) =>
      api.put(`/rankthresholds/${enc(rank)}`, { pointsRequired }),
    onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.rankThresholds }),
  })
}
