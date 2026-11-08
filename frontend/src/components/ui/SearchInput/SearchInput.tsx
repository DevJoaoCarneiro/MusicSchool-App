import type { InputHTMLAttributes } from 'react'
import { Search } from 'lucide-react'
import styles from './SearchInput.module.css'

export interface SearchInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label: string
}

export function SearchInput({ label, ...rest }: SearchInputProps) {
  return (
    <div className={styles.wrapper}>
      <Search className={styles.icon} size={18} strokeWidth={1.75} aria-hidden />
      <input type="search" className={styles.input} aria-label={label} {...rest} />
    </div>
  )
}
