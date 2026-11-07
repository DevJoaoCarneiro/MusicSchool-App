import type { ReactNode } from 'react'
import { cn } from '@/utils/cn'
import styles from './Badge.module.css'

export type BadgeTone = 'neutral' | 'success' | 'warning' | 'danger' | 'info'

export interface BadgeProps {
  tone?: BadgeTone
  children: ReactNode
}

export function Badge({ tone = 'neutral', children }: BadgeProps) {
  return <span className={cn(styles.badge, styles[tone])}>{children}</span>
}
