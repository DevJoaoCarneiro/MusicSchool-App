import { Music2 } from 'lucide-react'
import { cn } from '@/utils/cn'
import styles from './Brand.module.css'

export interface BrandProps {
  variant?: 'sidebar' | 'page'
  compact?: boolean
}

export function Brand({ variant = 'sidebar', compact = false }: BrandProps) {
  return (
    <span className={cn(styles.brand, styles[variant])}>
      <Music2 size={24} strokeWidth={1.75} aria-hidden />
      {!compact && <span className={styles.name}>{variant === 'page' ? 'Gambito Música' : 'Gambito'}</span>}
    </span>
  )
}
