import type { ReactNode } from 'react'
import styles from './DetailList.module.css'

export interface DetailItem {
  label: string
  value: ReactNode
  wide?: boolean
}

export interface DetailListProps {
  items: DetailItem[]
}

export function DetailList({ items }: DetailListProps) {
  return (
    <dl className={styles.list}>
      {items.map((item) => (
        <div key={item.label} className={item.wide ? styles.wide : undefined}>
          <dt className={styles.label}>{item.label}</dt>
          <dd className={styles.value}>{item.value || '—'}</dd>
        </div>
      ))}
    </dl>
  )
}
