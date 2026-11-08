import { createContext } from 'react'
import type { ApiResponse } from '@/types/api'
import type { AuthSession, AuthUser, LoginRequest } from '@/types/entities'

export interface AuthContextValue {
  user: AuthUser | null
  signIn: (request: LoginRequest) => Promise<ApiResponse<AuthSession>>
  signOut: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)
