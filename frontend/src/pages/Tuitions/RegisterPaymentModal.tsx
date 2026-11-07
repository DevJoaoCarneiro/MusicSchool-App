import { useState, type FormEvent } from 'react'
import { Button, FormRow, FormStack, Input, Modal, Select } from '@/components/ui'
import { optionsFrom, paymentMethods } from '@/config/labels'
import { useToast } from '@/hooks/useToast'
import { tuitionService } from '@/services/tuitionService'
import type { PaymentMethod, TuitionListItem } from '@/types/entities'
import { toIsoDate } from '@/utils/dates'
import { formatCurrency, formatReferenceMonth } from '@/utils/format'
import { brDateToIso, currencyToMask, isoToBrDate, maskCurrency, maskDate, parseCurrency } from '@/utils/masks'
import modalStyles from '../ModalContent.module.css'

interface RegisterPaymentModalProps {
  tuition: TuitionListItem
  onClose: () => void
  onSaved: () => void
}

const formId = 'register-payment-form'
const methodOptions = [{ value: '', label: 'Selecione' }, ...optionsFrom(paymentMethods)]

export function RegisterPaymentModal({ tuition, onClose, onSaved }: RegisterPaymentModalProps) {
  const { showToast } = useToast()
  const [paidAt, setPaidAt] = useState(() => isoToBrDate(toIsoDate(new Date())))
  const [amount, setAmount] = useState(() => currencyToMask(tuition.amount))
  const [method, setMethod] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    const response = await tuitionService.registerPayment(tuition.id, {
      paidAt: paidAt ? brDateToIso(paidAt) : null,
      amount: amount ? parseCurrency(amount) : null,
      method: (method || null) as PaymentMethod | null,
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
      title="Registrar pagamento"
      size="sm"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={submitting}>
            Cancelar
          </Button>
          <Button type="submit" form={formId} loading={submitting}>
            Registrar
          </Button>
        </>
      }
    >
      <form id={formId} onSubmit={handleSubmit}>
        <FormStack>
          <p className={modalStyles.notice}>
            {tuition.studentName} · {tuition.instrumentName} · {formatReferenceMonth(tuition.referenceMonth)} ·{' '}
            {formatCurrency(tuition.amount)}
          </p>
          <FormRow>
            <Input
              label="Data"
              placeholder="dd/mm/aaaa"
              inputMode="numeric"
              value={paidAt}
              onChange={(event) => setPaidAt(maskDate(event.target.value))}
            />
            <Input
              label="Valor (R$)"
              inputMode="numeric"
              value={amount}
              onChange={(event) => setAmount(maskCurrency(event.target.value))}
            />
          </FormRow>
          <Select
            label="Forma de pagamento"
            options={methodOptions}
            value={method}
            onChange={(event) => setMethod(event.target.value)}
          />
        </FormStack>
      </form>
    </Modal>
  )
}
