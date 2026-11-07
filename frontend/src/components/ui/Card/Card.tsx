import type { HTMLAttributes } from 'react'
import { cn } from '@/utils/cn'
import styles from './Card.module.css'

export function Card({ className, ...rest }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn(styles.card, className)} {...rest} />
}
