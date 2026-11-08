import type { ReactNode } from 'react'
import { cn } from '@/utils/cn'
import styles from './DataTable.module.css'

export interface Column<T> {
  key: string
  header: string
  render: (row: T) => ReactNode
  muted?: boolean
  align?: 'left' | 'right'
  width?: string
}

export interface DataTableProps<T> {
  columns: Column<T>[]
  rows: T[]
  rowKey: (row: T) => string
  loading?: boolean
  error?: string | null
  emptyMessage?: string
  footer?: ReactNode
}

export function DataTable<T>({
  columns,
  rows,
  rowKey,
  loading = false,
  error = null,
  emptyMessage = 'Nenhum registro encontrado',
  footer,
}: DataTableProps<T>) {
  const status = loading ? 'Carregando…' : error ?? (rows.length === 0 ? emptyMessage : null)

  return (
    <div className={styles.card}>
      <div className={styles.scroll}>
        <table className={styles.table}>
          <thead>
            <tr>
              {columns.map((column) => (
                <th
                  key={column.key}
                  scope="col"
                  style={{ width: column.width }}
                  className={cn(column.align === 'right' && styles.right)}
                >
                  {column.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {status ? (
              <tr>
                <td colSpan={columns.length} className={cn(styles.status, error && styles.error)}>
                  {status}
                </td>
              </tr>
            ) : (
              rows.map((row) => (
                <tr key={rowKey(row)}>
                  {columns.map((column) => (
                    <td
                      key={column.key}
                      className={cn(column.muted && styles.muted, column.align === 'right' && styles.right)}
                    >
                      {column.render(row)}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
      {footer && <div className={styles.footer}>{footer}</div>}
    </div>
  )
}
