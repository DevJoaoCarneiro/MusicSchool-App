import type { UserRole } from '@/types/entities'

export const roleLabels: Record<UserRole, string> = {
  Admin: 'Administrador',
  Secretary: 'Secretaria',
  Teacher: 'Professor',
  Student: 'Aluno',
  Guardian: 'Responsável',
}

export const allRoles: UserRole[] = ['Admin', 'Secretary', 'Teacher', 'Student', 'Guardian']

export const staffRoles: UserRole[] = ['Admin', 'Secretary', 'Teacher']

export const managerRoles: UserRole[] = ['Admin', 'Secretary']

export function hasRole(role: UserRole | undefined, allowed: UserRole[]) {
  return role !== undefined && allowed.includes(role)
}
