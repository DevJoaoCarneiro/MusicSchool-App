import { useState, type FormEvent } from 'react'
import { Button, Card, FormStack, Input, PageHeader } from '@/components/ui'
import { useToast } from '@/hooks/useToast'
import { authService } from '@/services/authService'
import styles from './ChangePasswordPage.module.css'

export function ChangePasswordPage() {
  const { showToast } = useToast()
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    const response = await authService.changePassword({ currentPassword, newPassword })
    setSubmitting(false)

    if (response.status !== 'Success') {
      showToast(response.message, 'error')
      return
    }

    showToast(response.message)
    setCurrentPassword('')
    setNewPassword('')
  }

  return (
    <div className={styles.container}>
      <PageHeader title="Trocar senha" description="Informe sua senha atual e a nova senha." />
      <Card>
        <form onSubmit={handleSubmit}>
          <FormStack>
            <Input
              label="Senha atual"
              type="password"
              autoComplete="current-password"
              value={currentPassword}
              onChange={(event) => setCurrentPassword(event.target.value)}
            />
            <Input
              label="Nova senha"
              type="password"
              autoComplete="new-password"
              value={newPassword}
              onChange={(event) => setNewPassword(event.target.value)}
            />
            <Button type="submit" fullWidth loading={submitting}>
              Salvar nova senha
            </Button>
          </FormStack>
        </form>
      </Card>
    </div>
  )
}
