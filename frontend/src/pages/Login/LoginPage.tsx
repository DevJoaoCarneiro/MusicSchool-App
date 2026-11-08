import { useState, type FormEvent } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { Brand, Button, Input } from '@/components/ui'
import { useAuth } from '@/hooks/useAuth'
import styles from './LoginPage.module.css'

export function LoginPage() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    setError(null)

    const response = await signIn({ email, password })
    setSubmitting(false)

    if (response.status !== 'Success') {
      setError(response.message)
      return
    }

    const from = (location.state as { from?: string } | null)?.from ?? '/'
    navigate(from, { replace: true })
  }

  return (
    <div className={styles.page}>
      <section className={styles.hero} aria-hidden>
        <blockquote className={styles.quote}>
          <p>“Onde há música, não há lugar para o mal.”</p>
          <footer>— Cervantes</footer>
        </blockquote>
      </section>

      <section className={styles.panel}>
        <form className={styles.form} onSubmit={handleSubmit}>
          <Brand variant="page" />

          <div className={styles.heading}>
            <h1 className={styles.title}>Bem-vindo de volta</h1>
            <p className={styles.subtitle}>Entre com seu e-mail e senha.</p>
          </div>

          <Input
            label="E-mail"
            type="email"
            autoComplete="email"
            placeholder="voce@escola.com"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            autoFocus
          />
          <Input
            label="Senha"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />

          {error && (
            <p className={styles.error} role="alert">
              {error}
            </p>
          )}

          <Button type="submit" fullWidth loading={submitting}>
            Entrar
          </Button>
        </form>
      </section>
    </div>
  )
}
