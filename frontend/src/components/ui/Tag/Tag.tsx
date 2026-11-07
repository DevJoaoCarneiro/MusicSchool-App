import type { ReactNode } from 'react'
import styles from './Tag.module.css'

export interface TagProps {
  children: ReactNode
}

export function Tag({ children }: TagProps) {
  return <span className={styles.tag}>{children}</span>
}

export interface TagListProps {
  items: string[]
}

export function TagList({ items }: TagListProps) {
  return (
    <span className={styles.list}>
      {items.map((item) => (
        <Tag key={item}>{item}</Tag>
      ))}
    </span>
  )
}
