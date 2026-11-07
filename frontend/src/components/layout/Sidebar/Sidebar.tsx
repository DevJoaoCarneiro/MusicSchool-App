import { NavLink } from 'react-router-dom'
import { KeyRound, LogOut, type LucideIcon } from 'lucide-react'
import { Brand } from '@/components/ui'
import type { NavItem } from '@/config/navigation'
import { cn } from '@/utils/cn'
import styles from './Sidebar.module.css'

export interface SidebarProps {
  items: NavItem[]
  open: boolean
  onNavigate: () => void
  onSignOut: () => void
}

interface SidebarLinkProps {
  to: string
  label: string
  icon: LucideIcon
  onNavigate: () => void
}

function SidebarLink({ to, label, icon: Icon, onNavigate }: SidebarLinkProps) {
  return (
    <NavLink
      to={to}
      end={to === '/'}
      title={label}
      onClick={onNavigate}
      className={({ isActive }) => cn(styles.link, isActive && styles.active)}
    >
      <Icon size={18} strokeWidth={1.75} aria-hidden />
      <span className={styles.label}>{label}</span>
    </NavLink>
  )
}

export function Sidebar({ items, open, onNavigate, onSignOut }: SidebarProps) {
  return (
    <aside className={cn(styles.sidebar, !open && styles.collapsed)} aria-label="Menu principal">
      <div className={styles.brand}>
        <Brand compact={!open} />
      </div>

      <nav className={styles.nav}>
        <span className={styles.sectionTitle}>Menu</span>
        {items.map((item) => (
          <SidebarLink
            key={item.path}
            to={item.path}
            label={item.label}
            icon={item.icon}
            onNavigate={onNavigate}
          />
        ))}
      </nav>

      <div className={styles.footer}>
        <SidebarLink to="/trocar-senha" label="Trocar senha" icon={KeyRound} onNavigate={onNavigate} />
        <button type="button" className={styles.link} title="Sair" onClick={onSignOut}>
          <LogOut size={18} strokeWidth={1.75} aria-hidden />
          <span className={styles.label}>Sair</span>
        </button>
      </div>
    </aside>
  )
}
