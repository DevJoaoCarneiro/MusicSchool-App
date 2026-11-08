import { useNavigate } from 'react-router-dom'
import { Button } from '@/components/ui'
import styles from '../StatusPage.module.css'

export function NotFoundPage() {
  const navigate = useNavigate()

  return (
    <div className={styles.inline}>
      <h1 className={styles.title}>Página não encontrada</h1>
      <p className={styles.text}>O endereço acessado não existe.</p>
      <Button variant="secondary" onClick={() => navigate('/')}>
        Ir para o início
      </Button>
    </div>
  )
}
