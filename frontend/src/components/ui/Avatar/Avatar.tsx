import { initials } from '@/utils/format'
import styles from './Avatar.module.css'

export interface AvatarProps {
  name: string
}

export function Avatar({ name }: AvatarProps) {
  return (
    <span className={styles.avatar} aria-hidden>
      {initials(name)}
    </span>
  )
}
