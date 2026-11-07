import { useState, type FormEvent } from 'react'
import { Button, FormRow, FormStack, Input, Modal, Select } from '@/components/ui'
import { useInstrumentOptions, useStudentOptions } from '@/hooks/useLookups'
import { useToast } from '@/hooks/useToast'
import { enrollmentService } from '@/services/enrollmentService'
import { brDateToIso, maskCurrency, maskDate, onlyDigits, parseCurrency } from '@/utils/masks'

interface EnrollmentFormModalProps {
  onClose: () => void
  onSaved: () => void
}

const formId = 'enrollment-form'

export function EnrollmentFormModal({ onClose, onSaved }: EnrollmentFormModalProps) {
  const { showToast } = useToast()
  const studentOptions = useStudentOptions('Selecione o aluno')
  const instrumentOptions = useInstrumentOptions('Selecione o instrumento')
  const [studentId, setStudentId] = useState('')
  const [instrumentId, setInstrumentId] = useState('')
  const [startDate, setStartDate] = useState('')
  const [monthlyFee, setMonthlyFee] = useState('')
  const [dueDay, setDueDay] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    const response = await enrollmentService.create({
      studentId,
      instrumentId,
      startDate: brDateToIso(startDate),
      monthlyFee: parseCurrency(monthlyFee),
      dueDay: Number(dueDay),
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
      title="Nova matrícula"
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
          <Select
            label="Aluno"
            options={studentOptions}
            value={studentId}
            onChange={(event) => setStudentId(event.target.value)}
            autoFocus
          />
          <Select
            label="Instrumento"
            options={instrumentOptions}
            value={instrumentId}
            onChange={(event) => setInstrumentId(event.target.value)}
          />
          <Input
            label="Data de início"
            placeholder="dd/mm/aaaa"
            inputMode="numeric"
            value={startDate}
            onChange={(event) => setStartDate(maskDate(event.target.value))}
          />
          <FormRow>
            <Input
              label="Mensalidade (R$)"
              placeholder="0,00"
              inputMode="numeric"
              value={monthlyFee}
              onChange={(event) => setMonthlyFee(maskCurrency(event.target.value))}
            />
            <Input
              label="Dia de vencimento"
              placeholder="1 a 28"
              inputMode="numeric"
              value={dueDay}
              onChange={(event) => setDueDay(onlyDigits(event.target.value).slice(0, 2))}
            />
          </FormRow>
        </FormStack>
      </form>
    </Modal>
  )
}
