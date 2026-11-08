import { useState, type FormEvent } from 'react'
import { Button, FormStack, Input, Modal } from '@/components/ui'
import { useToast } from '@/hooks/useToast'
import { enrollmentService } from '@/services/enrollmentService'
import { brDateToIso, maskDate } from '@/utils/masks'
import modalStyles from '../ModalContent.module.css'

interface CloseEnrollmentModalProps {
  enrollmentId: string
  label: string
  onClose: () => void
  onSaved: () => void
}

const formId = 'close-enrollment-form'

export function CloseEnrollmentModal({ enrollmentId, label, onClose, onSaved }: CloseEnrollmentModalProps) {
  const { showToast } = useToast()
  const [endDate, setEndDate] = useState('')
  const [reason, setReason] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    const response = await enrollmentService.close(enrollmentId, {
      endDate: endDate ? brDateToIso(endDate) : null,
      reason: reason.trim() || null,
    })
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
      title="Encerrar matrícula"
      size="sm"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={submitting}>
            Cancelar
          </Button>
          <Button type="submit" variant="danger" form={formId} loading={submitting}>
            Encerrar
          </Button>
        </>
      }
    >
      <form id={formId} onSubmit={handleSubmit}>
        <FormStack>
          <p className={modalStyles.notice}>{label}</p>
          <Input
            label="Data de término"
            placeholder="dd/mm/aaaa"
            inputMode="numeric"
            value={endDate}
            onChange={(event) => setEndDate(maskDate(event.target.value))}
            autoFocus
          />
          <Input label="Motivo" value={reason} onChange={(event) => setReason(event.target.value)} />
        </FormStack>
      </form>
    </Modal>
  )
}
