import { useState, type FormEvent } from 'react'
import { Button, FormStack, Input, Modal } from '@/components/ui'
import { useToast } from '@/hooks/useToast'
import { instrumentService } from '@/services/instrumentService'
import type { Instrument } from '@/types/entities'

interface InstrumentFormModalProps {
  instrument?: Instrument
  onClose: () => void
  onSaved: () => void
}

const formId = 'instrument-form'

export function InstrumentFormModal({ instrument, onClose, onSaved }: InstrumentFormModalProps) {
  const { showToast } = useToast()
  const [name, setName] = useState(instrument?.name ?? '')
  const [description, setDescription] = useState(instrument?.description ?? '')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)

    const request = { name, description }
    const response = instrument
      ? await instrumentService.update(instrument.id, request)
      : await instrumentService.create(request)

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
      title={instrument ? 'Editar instrumento' : 'Novo instrumento'}
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
          <Input label="Descrição" value={description} onChange={(event) => setDescription(event.target.value)} />
        </FormStack>
      </form>
    </Modal>
  )
}
