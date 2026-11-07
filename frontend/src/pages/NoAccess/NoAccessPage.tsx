import { useNavigate } from 'react-router-dom'
import { Brand, Button } from '@/components/ui'
import { roleLabels } from '@/config/roles'
import { useAuth } from '@/hooks/useAuth'
import styles from '../StatusPage.module.css'

export function NoAccessPage() {
  const { user, signOut } = useAuth()
  const navigate = useNavigate()

  function handleSignOut() {
    signOut()
    navigate('/login', { replace: true })
  }

  return (
    <div className={styles.page}>
      <div className={styles.box}>
        <Brand variant="page" />
        <h1 className={styles.title}>Área ainda não disponível</h1>
        <p className={styles.text}>
          {user
            ? `O perfil ${roleLabels[user.role]} ainda não tem uma área no sistema.`
            : 'Você precisa entrar com uma conta da escola.'}
        </p>
        <Button variant="secondary" onClick={handleSignOut}>
          Voltar para o login
        </Button>
      </div>
    </div>
  )
}
