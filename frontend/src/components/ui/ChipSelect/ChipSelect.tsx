import { useId } from 'react'
import { cn } from '@/utils/cn'
import styles from './ChipSelect.module.css'

export interface ChipSelectProps {
  label: string
  options: string[]
  value: string[]
  onChange: (value: string[]) => void
  emptyMessage?: string
}

export function ChipSelect({ label, options, value, onChange, emptyMessage }: ChipSelectProps) {
  const labelId = useId()

  function toggle(option: string) {
    onChange(value.includes(option) ? value.filter((item) => item !== option) : [...value, option])
  }

  return (
    <div className={styles.field} role="group" aria-labelledby={labelId}>
      <span id={labelId} className={styles.label}>
        {label}
      </span>
      <div className={styles.chips}>
        {options.length === 0 && emptyMessage && <span className={styles.empty}>{emptyMessage}</span>}
        {options.map((option) => {
          const selected = value.includes(option)
          return (
            <button
              key={option}
              type="button"
              className={cn(styles.chip, selected && styles.selected)}
              aria-pressed={selected}
              onClick={() => toggle(option)}
            >
              {option}
            </button>
          )
        })}
      </div>
    </div>
  )
}
