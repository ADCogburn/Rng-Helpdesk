import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../client'
import { queryKeys } from '../queryKeys'
import type {
  AdjustPointsRequest,
  GetUserResponse,
  GetUsersByHistoricalRsnResponse,
  GetUsersResponse,
  PointHistoryResponse,
  RenameRunescapeAccountRequest,
  RunescapeAccountHistoryResponse,
  RunescapeAccountsResponse,
  RunescapeUsernameRequest,
  UserLifecycleResponse,
} from '../types'

const enc = encodeURIComponent

export const useUsers = () =>
  useQuery({ queryKey: queryKeys.users.list, queryFn: () => api.get<GetUsersResponse>('/users') })

export const useUser = (id: string | undefined) =>
  useQuery({
    queryKey: queryKeys.users.detail(id ?? ''),
    queryFn: () => api.get<GetUserResponse>(`/users/${enc(id!)}`),
    enabled: !!id,
  })

export const useUserByRsn = (rsn: string | undefined) =>
  useQuery({
    queryKey: queryKeys.users.byRsn(rsn ?? ''),
    queryFn: () => api.get<GetUserResponse>(`/users/by-rsn/${enc(rsn!)}`),
    enabled: !!rsn,
    retry: false,
  })

export const useUsersByHistoricalRsn = (rsn: string | undefined) =>
  useQuery({
    queryKey: queryKeys.users.byHistoricalRsn(rsn ?? ''),
    queryFn: () =>
      api.get<GetUsersByHistoricalRsnResponse>(`/users/by-historical-rsn/${enc(rsn!)}`),
    enabled: !!rsn,
    retry: false,
  })

export const useUserLifecycle = (id: string | undefined) =>
  useQuery({
    queryKey: queryKeys.users.lifecycle(id ?? ''),
    queryFn: () => api.get<UserLifecycleResponse>(`/users/${enc(id!)}/lifecycle`),
    enabled: !!id,
  })

export const usePointHistory = (id: string | undefined) =>
  useQuery({
    queryKey: queryKeys.users.pointHistory(id ?? ''),
    queryFn: () => api.get<PointHistoryResponse>(`/users/${enc(id!)}/point-history`),
    enabled: !!id,
  })

export const useRunescapeAccounts = (id: string | undefined) =>
  useQuery({
    queryKey: queryKeys.users.rsAccounts(id ?? ''),
    queryFn: () => api.get<RunescapeAccountsResponse>(`/users/${enc(id!)}/runescape-accounts`),
    enabled: !!id,
  })

export const usePreviousRunescapeAccounts = (id: string | undefined) =>
  useQuery({
    queryKey: queryKeys.users.rsPrevious(id ?? ''),
    queryFn: () =>
      api.get<RunescapeAccountsResponse>(`/users/${enc(id!)}/runescape-accounts/previous`),
    enabled: !!id,
  })

export const useRunescapeAccountHistory = (id: string | undefined) =>
  useQuery({
    queryKey: queryKeys.users.rsHistory(id ?? ''),
    queryFn: () =>
      api.get<RunescapeAccountHistoryResponse>(`/users/${enc(id!)}/runescape-accounts/history`),
    enabled: !!id,
  })

/** Invalidates every cached query about one user plus the members list. */
function useInvalidateUser(id: string) {
  const qc = useQueryClient()
  return () =>
    Promise.all([
      qc.invalidateQueries({ queryKey: queryKeys.users.detail(id) }),
      qc.invalidateQueries({ queryKey: ['users', id] }),
      qc.invalidateQueries({ queryKey: queryKeys.users.list }),
      qc.invalidateQueries({ queryKey: ['public'] }),
    ])
}

export function useAddPoints(id: string) {
  const invalidate = useInvalidateUser(id)
  return useMutation({
    mutationFn: (body: AdjustPointsRequest) => api.post(`/users/${enc(id)}/points/add`, body),
    onSuccess: invalidate,
  })
}

export function useRemovePoints(id: string) {
  const invalidate = useInvalidateUser(id)
  return useMutation({
    mutationFn: (body: AdjustPointsRequest) => api.post(`/users/${enc(id)}/points/remove`, body),
    onSuccess: invalidate,
  })
}

export function useLinkRunescapeAccount(id: string) {
  const invalidate = useInvalidateUser(id)
  return useMutation({
    mutationFn: (body: RunescapeUsernameRequest) =>
      api.post(`/users/${enc(id)}/runescape-accounts`, body),
    onSuccess: invalidate,
  })
}

export function useDelinkRunescapeAccount(id: string) {
  const invalidate = useInvalidateUser(id)
  return useMutation({
    mutationFn: (body: RunescapeUsernameRequest) =>
      api.delete(`/users/${enc(id)}/runescape-accounts`, body),
    onSuccess: invalidate,
  })
}

export function useRenameRunescapeAccount(id: string) {
  const invalidate = useInvalidateUser(id)
  return useMutation({
    mutationFn: (body: RenameRunescapeAccountRequest) =>
      api.put(`/users/${enc(id)}/runescape-accounts/rename`, body),
    onSuccess: invalidate,
  })
}
