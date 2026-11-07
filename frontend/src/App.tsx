import { AuthProvider } from '@/contexts/AuthProvider'
import { ToastProvider } from '@/contexts/ToastProvider'
import { AppRouter } from '@/routes/AppRouter'

export function App() {
  return (
    <AuthProvider>
      <ToastProvider>
        <AppRouter />
      </ToastProvider>
    </AuthProvider>
  )
}
