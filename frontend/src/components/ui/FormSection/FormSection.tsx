import type { ReactNode } from 'react'
import styles from './FormSection.module.css'

export interface FormSectionProps {
  title: string
  hint?: string
  children: ReactNode
}

export function FormSection({ title, hint, children }: FormSectionProps) {
  return (
    <fieldset className={styles.section}>
      <legend className={styles.legend}>
        <span className={styles.title}>{title}</span>
        {hint && <span className={styles.hint}>{hint}</span>}
      </legend>
      {children}
    </fieldset>
  )
}

export interface FormRowProps {
  columns?: string
  children: ReactNode
}

export function FormRow({ columns = '1fr 1fr', children }: FormRowProps) {
  return (
    <div className={styles.row} style={{ gridTemplateColumns: columns }}>
      {children}
    </div>
  )
}

export function FormStack({ children }: { children: ReactNode }) {
  return <div className={styles.stack}>{children}</div>
}
