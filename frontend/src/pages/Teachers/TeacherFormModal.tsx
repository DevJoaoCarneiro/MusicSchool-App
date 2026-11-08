import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Button, ChipSelect, FormRow, FormStack, Input, Modal } from '@/components/ui'
import { useApiQuery } from '@/hooks/useApiQuery'
import { useToast } from '@/hooks/useToast'
import { instrumentService } from '@/services/instrumentService'
import { teacherService } from '@/services/teacherService'
import { maskCpf, maskPhone, onlyDigits } from '@/utils/masks'
import modalStyles from '../ModalContent.module.css'

interface TeacherFormModalProps {
  teacherId?: string
  onClose: () => void
  onSaved: () => void
}

const formId = 'teacher-form'

export function TeacherFormModal({ teacherId, onClose, onSaved }: TeacherFormModalProps) {
  const { showToast } = useToast()
  const instruments = useApiQuery(instrumentService.list)
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [cpf, setCpf] = useState('')
  const [specialties, setSpecialties] = useState<string[]>([])
  const [loading, setLoading] = useState(Boolean(teacherId))
  const [submitting, setSubmitting] = useState(false)
  const closeRef = useRef(onClose)

  useEffect(() => {
    closeRef.current = onClose
  }, [onClose])

  useEffect(() => {
    if (!teacherId) return

    let active = true
    teacherService.getById(teacherId).then((response) => {
      if (!active) return

      if (response.status === 'Success' && response.data) {
        setName(response.data.name)
        setEmail(response.data.email)
        setPhone(maskPhone(response.data.phone))
        setCpf(maskCpf(response.data.cpf))
        setSpecialties(response.data.specialties)
        setLoading(false)
      } else {
        showToast(response.message, 'error')
        closeRef.current()
      }
    })

    return () => {
      active = false
    }
  }, [teacherId, showToast])

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)

    const request = { name, email, phone: onlyDigits(phone), cpf: onlyDigits(cpf), specialties }
    const response = teacherId
      ? await teacherService.update(teacherId, request)
      : await teacherService.create(request)

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
      title={teacherId ? 'Editar professor' : 'Novo professor'}
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={submitting}>
            Cancelar
          </Button>
          <Button type="submit" form={formId} loading={submitting} disabled={loading}>
            Salvar
          </Button>
        </>
      }
    >
      {loading ? (
        <p className={modalStyles.loading}>Carregando…</p>
      ) : (
        <form id={formId} onSubmit={handleSubmit}>
          <FormStack>
            <Input label="Nome" autoFocus value={name} onChange={(event) => setName(event.target.value)} />
            <Input label="E-mail" type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
            <FormRow>
              <Input
                label="Telefone"
                placeholder="(00) 00000-0000"
                inputMode="tel"
                value={phone}
                onChange={(event) => setPhone(maskPhone(event.target.value))}
              />
              <Input
                label="CPF"
                placeholder="000.000.000-00"
                inputMode="numeric"
                value={cpf}
                onChange={(event) => setCpf(maskCpf(event.target.value))}
              />
            </FormRow>
            <ChipSelect
              label="Especialidades"
              options={(instruments.data ?? []).map((instrument) => instrument.name)}
              value={specialties}
              onChange={setSpecialties}
              emptyMessage={instruments.loading ? 'Carregando instrumentos…' : 'Nenhum instrumento cadastrado'}
            />
          </FormStack>
        </form>
      )}
    </Modal>
  )
}
