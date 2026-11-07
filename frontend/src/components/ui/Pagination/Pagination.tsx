import { ChevronLeft, ChevronRight } from 'lucide-react'
import { IconButton } from '../IconButton/IconButton'
import styles from './Pagination.module.css'

export interface PaginationProps {
  page: number
  pageSize: number
  total: number
  onPageChange: (page: number) => void
}

export function Pagination({ page, pageSize, total, onPageChange }: PaginationProps) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize))
  const start = total === 0 ? 0 : (page - 1) * pageSize + 1
  const end = Math.min(page * pageSize, total)

  return (
    <div className={styles.pagination}>
      <span className={styles.range}>
        {start}–{end} de {total}
      </span>
      <div className={styles.controls}>
        <IconButton
          icon={ChevronLeft}
          label="Página anterior"
          tone="bordered"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        />
        <span className={styles.page}>
          Página {page} de {totalPages}
        </span>
        <IconButton
          icon={ChevronRight}
          label="Próxima página"
          tone="bordered"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        />
      </div>
    </div>
  )
}
