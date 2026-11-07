import { useCallback, useMemo, useState, type ReactNode } from 'react'
import { sessionStorageKey as storageKey } from '@/config/storage'
import { authService } from '@/services/authService'
import type { AuthSession, LoginRequest } from '@/types/entities'
import { AuthContext, type AuthContextValue } from './auth-context'

function readSession(): AuthSession | null {
  try {
    const raw = sessionStorage.getItem(storageKey)
    return raw ? (JSON.parse(raw) as AuthSession) : null
  } catch {
    return null
  }
}

function writeSession(session: AuthSession | null) {
  try {
    if (session) {
      sessionStorage.setItem(storageKey, JSON.stringify(session))
    } else {
      sessionStorage.removeItem(storageKey)
    }
  } catch {
    return
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(readSession)

  const signIn = useCallback(async (request: LoginRequest) => {
    const response = await authService.login(request)

    if (response.status === 'Success' && response.data) {
      setSession(response.data)
      writeSession(response.data)
    }

    return response
  }, [])

  const signOut = useCallback(() => {
    setSession(null)
    writeSession(null)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({ user: session?.user ?? null, signIn, signOut }),
    [session, signIn, signOut],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
