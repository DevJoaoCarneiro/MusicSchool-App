import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { hasRole, staffRoles } from '@/config/roles'
import { useAuth } from '@/hooks/useAuth'
import type { UserRole } from '@/types/entities'

export function RequireAuth({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  const location = useLocation()

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  if (!hasRole(user.role, staffRoles)) {
    return <Navigate to="/sem-acesso" replace />
  }

  return children
}

export function RequireRole({ roles, children }: { roles: UserRole[]; children: ReactNode }) {
  const { user } = useAuth()

  if (!hasRole(user?.role, roles)) {
    return <Navigate to="/" replace />
  }

  return children
}

export function GuestOnly({ children }: { children: ReactNode }) {
  const { user } = useAuth()

  if (user) {
    return <Navigate to="/" replace />
  }

  return children
}
