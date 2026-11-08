import { useState, type FormEvent } from 'react'
import { Button, FormStack, Input, Modal } from '@/components/ui'
import { useToast } from '@/hooks/useToast'
import { tuitionService } from '@/services/tuitionService'
import type { GenerateTuitionsResult } from '@/types/entities'
import { formatReferenceMonth } from '@/utils/format'
import { brMonthToIso, isoToBrMonth, maskMonth } from '@/utils/masks'
import { toIsoMonth } from '@/utils/dates'
import modalStyles from '../ModalContent.module.css'

interface GenerateTuitionsModalProps {
  onClose: () => void
  onSaved: () => void
}

const formId = 'generate-tuitions-form'

export function GenerateTuitionsModal({ onClose, onSaved }: GenerateTuitionsModalProps) {
  const { showToast } = useToast()
  const [month, setMonth] = useState(() => isoToBrMonth(toIsoMonth(new Date())))
  const [submitting, setSubmitting] = useState(false)
  const [result, setResult] = useState<GenerateTuitionsResult | null>(null)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    const response = await tuitionService.generate(brMonthToIso(month))
    setSubmitting(false)

    if (response.status !== 'Success' || !response.data) {
      showToast(response.message, 'error')
      return
    }

    showToast(response.message)
    setResult(response.data)
    onSaved()
  }

  return (
    <Modal
      open
      title="Gerar mensalidades"
      size="sm"
      onClose={onClose}
      footer={
        result ? (
          <Button onClick={onClose}>Fechar</Button>
        ) : (
          <>
            <Button variant="secondary" onClick={onClose} disabled={submitting}>
              Cancelar
            </Button>
            <Button type="submit" form={formId} loading={submitting}>
              Gerar
            </Button>
          </>
        )
      }
    >
      {result ? (
        <FormStack>
          <p className={modalStyles.notice}>Mês de referência {formatReferenceMonth(result.referenceMonth)}</p>
          <div className={modalStyles.summary}>
            <div className={modalStyles.summaryItem}>
              <span className={modalStyles.summaryValue}>{result.created}</span>
              <span className={modalStyles.summaryLabel}>Criadas</span>
            </div>
            <div className={modalStyles.summaryItem}>
              <span className={modalStyles.summaryValue}>{result.skipped}</span>
              <span className={modalStyles.summaryLabel}>Já existentes</span>
            </div>
          </div>
        </FormStack>
      ) : (
        <form id={formId} onSubmit={handleSubmit}>
          <Input
            label="Mês de referência"
            placeholder="mm/aaaa"
            inputMode="numeric"
            value={month}
            onChange={(event) => setMonth(maskMonth(event.target.value))}
            autoFocus
          />
        </form>
      )}
    </Modal>
  )
}
