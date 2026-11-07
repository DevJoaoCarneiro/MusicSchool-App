import { useState } from 'react'
import { Link } from 'react-router-dom'
import { PageHeader } from '@/components/ui'
import { navItemsFor } from '@/config/navigation'
import { useAuth } from '@/hooks/useAuth'
import { firstName, formatDate } from '@/utils/format'
import styles from './HomePage.module.css'

export function HomePage() {
  const { user } = useAuth()
  const [today] = useState(() => new Date())

  if (!user) return null

  const shortcuts = navItemsFor(user.role).filter((item) => item.showOnHome)

  return (
    <>
      <PageHeader title={`Olá, ${firstName(user.name)}`} description={`Hoje é ${formatDate(today)}`} />

      <div className={styles.grid}>
        {shortcuts.map(({ path, label, description, icon: Icon }) => (
          <Link key={path} to={path} className={styles.card}>
            <Icon size={26} strokeWidth={1.5} className={styles.icon} aria-hidden />
            <span className={styles.title}>{label}</span>
            <span className={styles.description}>{description}</span>
          </Link>
        ))}
      </div>
    </>
  )
}
