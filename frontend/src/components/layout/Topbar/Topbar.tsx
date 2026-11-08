import { PanelLeft } from 'lucide-react'
import { Avatar, IconButton } from '@/components/ui'
import { roleLabels } from '@/config/roles'
import type { AuthUser } from '@/types/entities'
import styles from './Topbar.module.css'

export interface TopbarProps {
  user: AuthUser
  onToggleSidebar: () => void
}

export function Topbar({ user, onToggleSidebar }: TopbarProps) {
  return (
    <header className={styles.topbar}>
      <IconButton icon={PanelLeft} label="Alternar menu" onClick={onToggleSidebar} />
      <div className={styles.user}>
        <div className={styles.identity}>
          <span className={styles.name}>{user.name}</span>
          <span className={styles.role}>{roleLabels[user.role]}</span>
        </div>
        <Avatar name={user.name} />
      </div>
    </header>
  )
}
