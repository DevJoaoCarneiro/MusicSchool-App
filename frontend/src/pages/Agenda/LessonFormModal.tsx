import { useState, type FormEvent } from 'react'
import { Button, FormRow, FormStack, Input, Modal, Select } from '@/components/ui'
import { useActiveEnrollmentOptions, useTeacherOptions } from '@/hooks/useLookups'
import { useToast } from '@/hooks/useToast'
import { lessonService } from '@/services/lessonService'
import { combineDateTime } from '@/utils/dates'
import { brDateToIso, maskDate, maskTime, onlyDigits } from '@/utils/masks'

interface LessonFormModalProps {
  onClose: () => void
  onSaved: () => void
}

const formId = 'lesson-form'

export function LessonFormModal({ onClose, onSaved }: LessonFormModalProps) {
  const { showToast } = useToast()
  const enrollmentOptions = useActiveEnrollmentOptions('Selecione a matrícula')
  const teacherOptions = useTeacherOptions('Selecione o professor')
  const [enrollmentId, setEnrollmentId] = useState('')
  const [teacherId, setTeacherId] = useState('')
  const [date, setDate] = useState('')
  const [time, setTime] = useState('')
  const [duration, setDuration] = useState('60')
  const [room, setRoom] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    const response = await lessonService.create({
      enrollmentId,
      teacherId,
      startsAt: combineDateTime(brDateToIso(date), time),
      durationMinutes: Number(duration),
      room,
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
      title="Agendar aula"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={submitting}>
            Cancelar
          </Button>
          <Button type="submit" form={formId} loading={submitting}>
            Agendar
          </Button>
        </>
      }
    >
      <form id={formId} onSubmit={handleSubmit}>
        <FormStack>
          <Select
            label="Matrícula"
            options={enrollmentOptions}
            value={enrollmentId}
            onChange={(event) => setEnrollmentId(event.target.value)}
            autoFocus
          />
          <Select
            label="Professor"
            options={teacherOptions}
            value={teacherId}
            onChange={(event) => setTeacherId(event.target.value)}
          />
          <FormRow>
            <Input
              label="Data"
              placeholder="dd/mm/aaaa"
              inputMode="numeric"
              value={date}
              onChange={(event) => setDate(maskDate(event.target.value))}
            />
            <Input
              label="Horário"
              placeholder="00:00"
              inputMode="numeric"
              value={time}
              onChange={(event) => setTime(maskTime(event.target.value))}
            />
          </FormRow>
          <FormRow>
            <Input
              label="Duração (min)"
              placeholder="30 a 120"
              inputMode="numeric"
              value={duration}
              onChange={(event) => setDuration(onlyDigits(event.target.value).slice(0, 3))}
            />
            <Input label="Sala" value={room} onChange={(event) => setRoom(event.target.value)} />
          </FormRow>
        </FormStack>
      </form>
    </Modal>
  )
}
