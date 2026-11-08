import { useCallback, useMemo, useState, type ReactNode } from 'react'
import { ToastViewport, type ToastItem } from '@/components/ui/Toast/Toast'
import { ToastContext, type ToastContextValue, type ToastTone } from './toast-context'

const toastDurationMs = 4000

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([])

  const dismiss = useCallback((id: number) => {
    setToasts((current) => current.filter((toast) => toast.id !== id))
  }, [])

  const showToast = useCallback(
    (message: string, tone: ToastTone = 'success') => {
      const id = Date.now() + Math.random()
      setToasts((current) => [...current, { id, message, tone }])
      setTimeout(() => dismiss(id), toastDurationMs)
    },
    [dismiss],
  )

  const value = useMemo<ToastContextValue>(() => ({ showToast }), [showToast])

  return (
    <ToastContext.Provider value={value}>
      {children}
      <ToastViewport toasts={toasts} onDismiss={dismiss} />
    </ToastContext.Provider>
  )
}
