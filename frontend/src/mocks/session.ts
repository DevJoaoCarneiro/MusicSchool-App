import { sessionStorageKey } from '@/config/storage'
import type { AuthSession } from '@/types/entities'
import { teachers } from './db'

export function currentMockUser() {
  try {
    const raw = sessionStorage.getItem(sessionStorageKey)
    return raw ? (JSON.parse(raw) as AuthSession).user : null
  } catch {
    return null
  }
}

export function currentTeacherScope() {
  const user = currentMockUser()
  if (user?.role !== 'Teacher') return { scoped: false as const }

  const teacher = teachers.find((item) => item.email.toLowerCase() === user.email.toLowerCase())
  return { scoped: true as const, teacherId: teacher?.id ?? null }
}
