import { CircleAlert, CircleCheck, X } from 'lucide-react'
import { cn } from '@/utils/cn'
import styles from './Toast.module.css'

export interface ToastItem {
  id: number
  message: string
  tone: 'success' | 'error'
}

export interface ToastViewportProps {
  toasts: ToastItem[]
  onDismiss: (id: number) => void
}

export function ToastViewport({ toasts, onDismiss }: ToastViewportProps) {
  return (
    <div className={styles.viewport} aria-live="polite" role="status">
      {toasts.map((toast) => {
        const Icon = toast.tone === 'success' ? CircleCheck : CircleAlert
        return (
          <div key={toast.id} className={cn(styles.toast, styles[toast.tone])}>
            <Icon size={18} strokeWidth={1.75} aria-hidden />
            <span className={styles.message}>{toast.message}</span>
            <button
              type="button"
              className={styles.close}
              aria-label="Fechar aviso"
              onClick={() => onDismiss(toast.id)}
            >
              <X size={16} aria-hidden />
            </button>
          </div>
        )
      })}
    </div>
  )
}
