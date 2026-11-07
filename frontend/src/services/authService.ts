import type { AuthSession, ChangePasswordRequest, LoginRequest } from '@/types/entities'
import { http } from './http'

export const authService = {
  login: (request: LoginRequest) => http.post<AuthSession>('/auth/login', request),

  changePassword: (request: ChangePasswordRequest) => http.patch<null>('/users/me/password', request),
}
