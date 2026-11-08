import { useState, type FormEvent } from 'react'
import { Button, FormRow, FormStack, Input, Modal, Select } from '@/components/ui'
import { weekdays } from '@/config/labels'
import { useActiveEnrollmentOptions, useTeacherOptions } from '@/hooks/useLookups'
import { useToast } from '@/hooks/useToast'
import { lessonService } from '@/services/lessonService'
import type { LessonConflicts, RecurringLessonResult, Weekday } from '@/types/entities'
import { formatDate } from '@/utils/format'
import { brDateToIso, maskDate, maskTime, onlyDigits } from '@/utils/masks'
import modalStyles from '../ModalContent.module.css'

interface RecurringLessonModalProps {
  onClose: () => void
  onSaved: () => void
}

const formId = 'recurring-lesson-form'
const weekdayOptions = [{ value: '', label: 'Selecione o dia' }, ...weekdays]

function hasConflicts(data: RecurringLessonResult | LessonConflicts | null): data is LessonConflicts {
  return data !== null && 'conflictingDates' in data
}

export function RecurringLessonModal({ onClose, onSaved }: RecurringLessonModalProps) {
  const { showToast } = useToast()
  const enrollmentOptions = useActiveEnrollmentOptions('Selecione a matrícula')
  const teacherOptions = useTeacherOptions('Selecione o professor')
  const [enrollmentId, setEnrollmentId] = useState('')
  const [teacherId, setTeacherId] = useState('')
  const [weekday, setWeekday] = useState('')
  const [time, setTime] = useState('')
  const [duration, setDuration] = useState('60')
  const [room, setRoom] = useState('')
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [conflicts, setConflicts] = useState<string[]>([])
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    setConflicts([])

    const response = await lessonService.createRecurring({
      enrollmentId,
      teacherId,
      weekday: weekday as Weekday,
      time,
      durationMinutes: Number(duration),
      room,
      startDate: brDateToIso(startDate),
      endDate: brDateToIso(endDate),
    })

    setSubmitting(false)

    if (response.status !== 'Success') {
      if (hasConflicts(response.data)) setConflicts(response.data.conflictingDates)
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
      title="Aulas recorrentes"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={submitting}>
            Cancelar
          </Button>
          <Button type="submit" form={formId} loading={submitting}>
            Agendar série
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
          <FormRow columns="1.4fr 1fr 1fr">
            <Select
              label="Dia da semana"
              options={weekdayOptions}
              value={weekday}
              onChange={(event) => setWeekday(event.target.value)}
            />
            <Input
              label="Horário"
              placeholder="00:00"
              inputMode="numeric"
              value={time}
              onChange={(event) => setTime(maskTime(event.target.value))}
            />
            <Input
              label="Duração (min)"
              inputMode="numeric"
              value={duration}
              onChange={(event) => setDuration(onlyDigits(event.target.value).slice(0, 3))}
            />
          </FormRow>
          <FormRow columns="1fr 1fr 1fr">
            <Input label="Sala" value={room} onChange={(event) => setRoom(event.target.value)} />
            <Input
              label="Início"
              placeholder="dd/mm/aaaa"
              inputMode="numeric"
              value={startDate}
              onChange={(event) => setStartDate(maskDate(event.target.value))}
            />
            <Input
              label="Fim"
              placeholder="dd/mm/aaaa"
              inputMode="numeric"
              value={endDate}
              onChange={(event) => setEndDate(maskDate(event.target.value))}
            />
          </FormRow>
        </FormStack>

        {conflicts.length > 0 && (
          <div className={modalStyles.conflicts} role="alert">
            <strong>Datas em conflito</strong>
            <ul className={modalStyles.conflictList}>
              {conflicts.map((date) => (
                <li key={date}>{formatDate(date)}</li>
              ))}
            </ul>
          </div>
        )}
      </form>
    </Modal>
  )
}
