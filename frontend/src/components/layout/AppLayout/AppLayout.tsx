import { useCallback, useState } from 'react'
import { Outlet, useNavigate } from 'react-router-dom'
import { navItemsFor } from '@/config/navigation'
import { useAuth } from '@/hooks/useAuth'
import { Sidebar } from '../Sidebar/Sidebar'
import { Topbar } from '../Topbar/Topbar'
import styles from './AppLayout.module.css'

const mobileQuery = '(max-width: 900px)'

function isMobile() {
  return window.matchMedia(mobileQuery).matches
}

export function AppLayout() {
  const { user, signOut } = useAuth()
  const navigate = useNavigate()
  const [sidebarOpen, setSidebarOpen] = useState(() => !isMobile())

  const closeOnMobile = useCallback(() => {
    if (isMobile()) setSidebarOpen(false)
  }, [])

  const handleSignOut = useCallback(() => {
    signOut()
    navigate('/login', { replace: true })
  }, [signOut, navigate])

  if (!user) return null

  return (
    <div className={styles.shell}>
      <Sidebar
        items={navItemsFor(user.role)}
        open={sidebarOpen}
        onNavigate={closeOnMobile}
        onSignOut={handleSignOut}
      />
      {sidebarOpen && <div className={styles.backdrop} onClick={() => setSidebarOpen(false)} aria-hidden />}
      <div className={styles.main}>
        <Topbar user={user} onToggleSidebar={() => setSidebarOpen((open) => !open)} />
        <main className={styles.content}>
          <Outlet />
        </main>
      </div>
    </div>
  )
}
