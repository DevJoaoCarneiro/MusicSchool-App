import { useState, type FormEvent } from 'react'
import { Button, FormStack, Input, Modal, Select } from '@/components/ui'
import { allRoles, roleLabels } from '@/config/roles'
import { useToast } from '@/hooks/useToast'
import { userService } from '@/services/userService'
import type { UserRole } from '@/types/entities'

interface UserFormModalProps {
  onClose: () => void
  onSaved: () => void
}

const formId = 'user-form'
const roleOptions = allRoles.map((role) => ({ value: role, label: roleLabels[role] }))

export function UserFormModal({ onClose, onSaved }: UserFormModalProps) {
  const { showToast } = useToast()
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState<UserRole>('Secretary')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    const response = await userService.create({ name, email, password, role })
    setSubmitting(false)

    if (response.status !== 'Success') {
      showToast(response.message, 'error')
      return
    }

    showToast(response.message)
    onSaved()
    onClose()
  }

  return (
    <Modal
      open
      title="Novo usuário"
      size="sm"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={submitting}>
            Cancelar
          </Button>
          <Button type="submit" form={formId} loading={submitting}>
            Salvar
          </Button>
        </>
      }
    >
      <form id={formId} onSubmit={handleSubmit}>
        <FormStack>
          <Input label="Nome" autoFocus value={name} onChange={(event) => setName(event.target.value)} />
          <Input label="E-mail" type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
          <Input
            label="Senha"
            type="password"
            autoComplete="new-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
          <Select
            label="Perfil"
            options={roleOptions}
            value={role}
            onChange={(event) => setRole(event.target.value as UserRole)}
          />
        </FormStack>
      </form>
    </Modal>
  )
}
