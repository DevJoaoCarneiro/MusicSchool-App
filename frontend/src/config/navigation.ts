import {
  CalendarDays,
  ClipboardList,
  GraduationCap,
  Guitar,
  House,
  UserCog,
  Users,
  Wallet,
  type LucideIcon,
} from 'lucide-react'
import type { UserRole } from '@/types/entities'
import { managerRoles, staffRoles } from './roles'

export interface NavItem {
  label: string
  description: string
  path: string
  icon: LucideIcon
  roles: UserRole[]
  showOnHome: boolean
}

export const navItems: NavItem[] = [
  {
    label: 'Início',
    description: '',
    path: '/',
    icon: House,
    roles: staffRoles,
    showOnHome: false,
  },
  {
    label: 'Alunos',
    description: 'Cadastro e consulta de alunos',
    path: '/alunos',
    icon: GraduationCap,
    roles: staffRoles,
    showOnHome: true,
  },
  {
    label: 'Professores',
    description: 'Equipe e especialidades',
    path: '/professores',
    icon: Users,
    roles: staffRoles,
    showOnHome: true,
  },
  {
    label: 'Instrumentos',
    description: 'Catálogo de instrumentos',
    path: '/instrumentos',
    icon: Guitar,
    roles: staffRoles,
    showOnHome: true,
  },
  {
    label: 'Matrículas',
    description: 'Alunos matriculados por instrumento',
    path: '/matriculas',
    icon: ClipboardList,
    roles: staffRoles,
    showOnHome: true,
  },
  {
    label: 'Agenda',
    description: 'Aulas da semana e do mês',
    path: '/agenda',
    icon: CalendarDays,
    roles: staffRoles,
    showOnHome: true,
  },
  {
    label: 'Mensalidades',
    description: 'Cobranças e pagamentos',
    path: '/mensalidades',
    icon: Wallet,
    roles: managerRoles,
    showOnHome: true,
  },
  {
    label: 'Usuários',
    description: 'Contas de acesso ao sistema',
    path: '/usuarios',
    icon: UserCog,
    roles: ['Admin'],
    showOnHome: false,
  },
]

export function navItemsFor(role: UserRole) {
  return navItems.filter((item) => item.roles.includes(role))
}
