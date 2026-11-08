import { useState, type FormEvent } from 'react'
import { Button, FormRow, FormStack, Input, Modal } from '@/components/ui'
import { useToast } from '@/hooks/useToast'
import { lessonService } from '@/services/lessonService'
import type { LessonDetail } from '@/types/entities'
import { combineDateTime } from '@/utils/dates'
import { formatDateTime } from '@/utils/format'
import { brDateToIso, isoToBrDate, maskDate, maskTime } from '@/utils/masks'
import modalStyles from '../ModalContent.module.css'

interface RescheduleLessonModalProps {
  lesson: LessonDetail
  onClose: () => void
  onSaved: () => void
}

const formId = 'reschedule-lesson-form'

export function RescheduleLessonModal({ lesson, onClose, onSaved }: RescheduleLessonModalProps) {
  const { showToast } = useToast()
  const [date, setDate] = useState(() => isoToBrDate(lesson.startsAt))
  const [time, setTime] = useState(() => lesson.startsAt.slice(11, 16))
  const [room, setRoom] = useState(lesson.room)
  const [reason, setReason] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    const response = await lessonService.reschedule(lesson.id, {
      newStartsAt: combineDateTime(brDateToIso(date), time),
      room,
      reason,
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
      title="Reagendar aula"
      size="sm"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={submitting}>
            Cancelar
          </Button>
          <Button type="submit" form={formId} loading={submitting}>
            Reagendar
          </Button>
        </>
      }
    >
      <form id={formId} onSubmit={handleSubmit}>
        <FormStack>
          <p className={modalStyles.notice}>
            {lesson.student.name} · {lesson.instrument} · atualmente {formatDateTime(lesson.startsAt)}
          </p>
          <FormRow>
            <Input
              label="Nova data"
              placeholder="dd/mm/aaaa"
              inputMode="numeric"
              value={date}
              onChange={(event) => setDate(maskDate(event.target.value))}
              autoFocus
            />
            <Input
              label="Novo horário"
              placeholder="00:00"
              inputMode="numeric"
              value={time}
              onChange={(event) => setTime(maskTime(event.target.value))}
            />
          </FormRow>
          <Input label="Sala" value={room} onChange={(event) => setRoom(event.target.value)} />
          <Input label="Motivo" value={reason} onChange={(event) => setReason(event.target.value)} />
        </FormStack>
      </form>
    </Modal>
  )
}
