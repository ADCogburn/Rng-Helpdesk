import { useMutation, useQuery } from '@tanstack/react-query'
import { api } from '../client'
import { queryKeys } from '../queryKeys'
import { getToken } from '../session'
import type { ChangePasswordRequest, GetUserResponse, LoginRequest, LoginResponse } from '../types'

export const login = (body: LoginRequest) =>
  api.post<LoginResponse>('/auth/login', body, { skipAuthRedirect: true })

export const fetchMe = () => api.get<GetUserResponse>('/auth/me')

export function useLogin() {
  return useMutation({ mutationFn: login })
}

/** Current user from `/auth/me`; disabled when there is no stored token. */
export function useMe() {
  return useQuery({
    queryKey: queryKeys.auth.me,
    queryFn: fetchMe,
    enabled: !!getToken(),
    retry: false,
    staleTime: 60_000,
  })
}

export function useChangePassword() {
  return useMutation({
    mutationFn: (body: ChangePasswordRequest) => api.post('/auth/change-password', body),
  })
}
