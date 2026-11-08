import { useCallback, useState } from 'react'
import { Badge, Button, DetailList, Modal } from '@/components/ui'
import { lessonStatus } from '@/config/labels'
import { useApiQuery } from '@/hooks/useApiQuery'
import { useToast } from '@/hooks/useToast'
import { lessonService } from '@/services/lessonService'
import type { LessonDetail } from '@/types/entities'
import { formatDate, formatDateTime, formatTime } from '@/utils/format'
import modalStyles from '../ModalContent.module.css'

interface LessonDetailModalProps {
  lessonId: string
  canManage: boolean
  onChanged: () => void
  onReschedule: (lesson: LessonDetail) => void
  onCancel: (lesson: LessonDetail) => void
  onCancelSeries: (lesson: LessonDetail) => void
  onClose: () => void
}

export function LessonDetailModal({
  lessonId,
  canManage,
  onChanged,
  onReschedule,
  onCancel,
  onCancelSeries,
  onClose,
}: LessonDetailModalProps) {
  const { showToast } = useToast()
  const fetcher = useCallback(() => lessonService.getById(lessonId), [lessonId])
  const { data: lesson, loading, error } = useApiQuery(fetcher)
  const [completing, setCompleting] = useState(false)
  const showActions = canManage && lesson?.status === 'Scheduled'

  async function handleComplete() {
    setCompleting(true)
    const response = await lessonService.complete(lessonId)
    setCompleting(false)

    showToast(response.message, response.status === 'Success' ? 'success' : 'error')

    if (response.status === 'Success') {
      onChanged()
      onClose()
    }
  }

  return (
    <Modal
      open
      title="Detalhes da aula"
      size="lg"
      onClose={onClose}
      footer={
        <div className={modalStyles.footerSpread}>
          <Button variant="secondary" onClick={onClose}>
            Fechar
          </Button>
          {showActions && lesson && (
            <>
              {lesson.seriesId && (
                <Button variant="ghost" onClick={() => onCancelSeries(lesson)}>
                  Cancelar série
                </Button>
              )}
              <Button variant="secondary" onClick={() => onCancel(lesson)}>
                Cancelar
              </Button>
              <Button variant="secondary" onClick={() => onReschedule(lesson)}>
                Reagendar
              </Button>
              <Button onClick={handleComplete} loading={completing}>
                Concluir
              </Button>
            </>
          )}
        </div>
      }
    >
      {loading && <p className={modalStyles.loading}>Carregando…</p>}
      {error && <p className={modalStyles.loading}>{error}</p>}
      {lesson && (
        <>
          <DetailList
            items={[
              { label: 'Data', value: formatDate(lesson.startsAt) },
              { label: 'Horário', value: `${formatTime(lesson.startsAt)} – ${formatTime(lesson.endsAt)}` },
              { label: 'Aluno', value: lesson.student.name },
              { label: 'Instrumento', value: lesson.instrument },
              { label: 'Professor', value: lesson.teacher.name },
              { label: 'Sala', value: lesson.room },
              {
                label: 'Status',
                value: <Badge tone={lessonStatus[lesson.status].tone}>{lessonStatus[lesson.status].label}</Badge>,
              },
              { label: 'Recorrência', value: lesson.seriesId ? 'Faz parte de uma série' : 'Aula avulsa' },
            ]}
          />
          {lesson.reason && (
            <p className={modalStyles.notice}>
              <strong>Motivo:</strong> {lesson.reason}
              {lesson.cancelledAt && ` · cancelada em ${formatDateTime(lesson.cancelledAt)}`}
            </p>
          )}
        </>
      )}
    </Modal>
  )
}
