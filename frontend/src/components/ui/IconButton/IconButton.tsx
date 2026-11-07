import type { ButtonHTMLAttributes } from 'react'
import type { LucideIcon } from 'lucide-react'
import { cn } from '@/utils/cn'
import styles from './IconButton.module.css'

export interface IconButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  icon: LucideIcon
  label: string
  tone?: 'default' | 'danger' | 'bordered'
}

export function IconButton({
  icon: Icon,
  label,
  tone = 'default',
  className,
  type = 'button',
  ...rest
}: IconButtonProps) {
  return (
    <button
      type={type}
      className={cn(styles.iconButton, styles[tone], className)}
      aria-label={label}
      title={label}
      {...rest}
    >
      <Icon size={18} strokeWidth={1.75} aria-hidden />
    </button>
  )
}
