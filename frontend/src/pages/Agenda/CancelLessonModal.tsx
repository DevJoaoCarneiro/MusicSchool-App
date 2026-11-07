import { useState, type FormEvent } from 'react'
import { Button, FormStack, Input, Modal } from '@/components/ui'
import { useToast } from '@/hooks/useToast'
import { lessonService } from '@/services/lessonService'
import type { LessonDetail } from '@/types/entities'
import { formatDateTime } from '@/utils/format'
import modalStyles from '../ModalContent.module.css'

interface CancelLessonModalProps {
  lesson: LessonDetail
  mode: 'single' | 'series'
  onClose: () => void
  onSaved: () => void
}

const formId = 'cancel-lesson-form'

export function CancelLessonModal({ lesson, mode, onClose, onSaved }: CancelLessonModalProps) {
  const { showToast } = useToast()
  const [reason, setReason] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const isSeries = mode === 'series'

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)

    const response =
      isSeries && lesson.seriesId
        ? await lessonService.cancelSeries(lesson.seriesId, { reason })
        : await lessonService.cancel(lesson.id, { reason })

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
      title={isSeries ? 'Cancelar série' : 'Cancelar aula'}
      size="sm"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={submitting}>
            Voltar
          </Button>
          <Button type="submit" variant="danger" form={formId} loading={submitting}>
            {isSeries ? 'Cancelar série' : 'Cancelar aula'}
          </Button>
        </>
      }
    >
      <form id={formId} onSubmit={handleSubmit}>
        <FormStack>
          <p className={modalStyles.notice}>
            {isSeries
              ? `Todas as aulas agendadas desta série de ${lesson.student.name} (${lesson.instrument}) serão canceladas.`
              : `${lesson.student.name} · ${lesson.instrument} · ${formatDateTime(lesson.startsAt)}`}
          </p>
          <Input label="Motivo" value={reason} onChange={(event) => setReason(event.target.value)} autoFocus />
        </FormStack>
      </form>
    </Modal>
  )
}
