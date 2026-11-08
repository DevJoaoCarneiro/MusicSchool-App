import { useMemo, useState } from 'react'
import { CalendarPlus, ChevronLeft, ChevronRight, Repeat } from 'lucide-react'
import { Badge, Button, FilterBar, IconButton, PageHeader, SegmentedControl, Select } from '@/components/ui'
import { lessonStatus } from '@/config/labels'
import { hasRole, managerRoles } from '@/config/roles'
import { useApiQuery } from '@/hooks/useApiQuery'
import { useAuth } from '@/hooks/useAuth'
import { useStudentOptions, useTeacherOptions } from '@/hooks/useLookups'
import { lessonService } from '@/services/lessonService'
import type { LessonDetail, LessonListItem } from '@/types/entities'
import { cn } from '@/utils/cn'
import { addDays, startOfDay, startOfMonth, startOfWeek, toIsoDate } from '@/utils/dates'
import { formatMonthTitle } from '@/utils/format'
import styles from './Agenda.module.css'
import { MonthView, WeekView } from './CalendarViews'
import { CancelLessonModal } from './CancelLessonModal'
import { LessonDetailModal } from './LessonDetailModal'
import { LessonFormModal } from './LessonFormModal'
import { RecurringLessonModal } from './RecurringLessonModal'
import { RescheduleLessonModal } from './RescheduleLessonModal'

type View = 'week' | 'month'

type ModalState =
  | { kind: 'detail'; id: string }
  | { kind: 'schedule' }
  | { kind: 'recurring' }
  | { kind: 'reschedule'; lesson: LessonDetail }
  | { kind: 'cancel'; lesson: LessonDetail }
  | { kind: 'cancelSeries'; lesson: LessonDetail }

const viewOptions = [
  { value: 'week' as const, label: 'Semana' },
  { value: 'month' as const, label: 'Mês' },
]

function rangeFor(view: View, anchor: Date) {
  if (view === 'week') {
    const start = startOfWeek(anchor)
    return { start, end: addDays(start, 6) }
  }

  const start = startOfWeek(startOfMonth(anchor))
  return { start, end: addDays(start, 41) }
}

function weekLabel(start: Date) {
  const end = addDays(start, 6)
  const startText = start.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' })
  const endText = end.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' })
  return `${startText} – ${endText}`.replaceAll('.', '')
}

export function AgendaPage() {
  const { user } = useAuth()
  const isTeacher = user?.role === 'Teacher'
  const canManage = hasRole(user?.role, managerRoles)
  const [today] = useState(() => startOfDay(new Date()))
  const [view, setView] = useState<View>('week')
  const [anchor, setAnchor] = useState(today)
  const [teacherId, setTeacherId] = useState('')
  const [studentId, setStudentId] = useState('')
  const [modal, setModal] = useState<ModalState | null>(null)
  const teacherOptions = useTeacherOptions('Todos')
  const studentOptions = useStudentOptions('Todos')

  const { start, end } = useMemo(() => rangeFor(view, anchor), [view, anchor])

  const fetcher = useMemo(() => {
    const params = {
      from: toIsoDate(start),
      to: toIsoDate(end),
      teacherId: teacherId || undefined,
      studentId: studentId || undefined,
    }
    return () => lessonService.listByRange(params)
  }, [start, end, teacherId, studentId])

  const { data, loading, error, reload } = useApiQuery(fetcher)
  const lessons = data ?? []
  const closeModal = () => setModal(null)

  function move(direction: 1 | -1) {
    setAnchor((current) =>
      view === 'week'
        ? addDays(current, direction * 7)
        : new Date(current.getFullYear(), current.getMonth() + direction, 1),
    )
  }

  function showWeekOf(day: Date) {
    setAnchor(day)
    setView('week')
  }

  const selectLesson = (lesson: LessonListItem) => setModal({ kind: 'detail', id: lesson.id })

  return (
    <>
      <PageHeader
        title={isTeacher ? 'Minha agenda' : 'Agenda'}
        description="Aulas da semana e do mês"
        actions={
          canManage && (
            <>
              <Button variant="secondary" icon={Repeat} onClick={() => setModal({ kind: 'recurring' })}>
                Aulas recorrentes
              </Button>
              <Button icon={CalendarPlus} onClick={() => setModal({ kind: 'schedule' })}>
                Agendar aula
              </Button>
            </>
          )
        }
      />

      <FilterBar>
        {!isTeacher && (
          <Select
            label="Professor"
            options={teacherOptions}
            value={teacherId}
            onChange={(event) => setTeacherId(event.target.value)}
          />
        )}
        <Select
          label="Aluno"
          options={studentOptions}
          value={studentId}
          onChange={(event) => setStudentId(event.target.value)}
        />
      </FilterBar>

      <div className={styles.toolbar}>
        <div className={styles.navigation}>
          <IconButton icon={ChevronLeft} label="Período anterior" tone="bordered" onClick={() => move(-1)} />
          <Button variant="secondary" onClick={() => setAnchor(today)}>
            Hoje
          </Button>
          <IconButton icon={ChevronRight} label="Próximo período" tone="bordered" onClick={() => move(1)} />
          <span className={styles.rangeLabel} aria-live="polite">
            {view === 'week' ? weekLabel(start) : formatMonthTitle(anchor)}
          </span>
        </div>
        <SegmentedControl label="Visualização" options={viewOptions} value={view} onChange={setView} />
      </div>

      {error ? (
        <p className={cn(styles.status, styles.error)}>{error}</p>
      ) : loading && !data ? (
        <p className={styles.status}>Carregando…</p>
      ) : view === 'week' ? (
        <WeekView start={start} today={today} lessons={lessons} onSelect={selectLesson} />
      ) : (
        <MonthView
          start={start}
          today={today}
          month={anchor.getMonth()}
          lessons={lessons}
          onSelect={selectLesson}
          onShowDay={showWeekOf}
        />
      )}

      <div className={styles.legend}>
        {Object.values(lessonStatus).map((status) => (
          <Badge key={status.label} tone={status.tone}>
            {status.label}
          </Badge>
        ))}
      </div>

      {modal?.kind === 'detail' && (
        <LessonDetailModal
          lessonId={modal.id}
          canManage={canManage}
          onChanged={reload}
          onReschedule={(lesson) => setModal({ kind: 'reschedule', lesson })}
          onCancel={(lesson) => setModal({ kind: 'cancel', lesson })}
          onCancelSeries={(lesson) => setModal({ kind: 'cancelSeries', lesson })}
          onClose={closeModal}
        />
      )}

      {modal?.kind === 'schedule' && <LessonFormModal onClose={closeModal} onSaved={reload} />}

      {modal?.kind === 'recurring' && <RecurringLessonModal onClose={closeModal} onSaved={reload} />}

      {modal?.kind === 'reschedule' && (
        <RescheduleLessonModal lesson={modal.lesson} onClose={closeModal} onSaved={reload} />
      )}

      {modal?.kind === 'cancel' && (
        <CancelLessonModal lesson={modal.lesson} mode="single" onClose={closeModal} onSaved={reload} />
      )}

      {modal?.kind === 'cancelSeries' && (
        <CancelLessonModal lesson={modal.lesson} mode="series" onClose={closeModal} onSaved={reload} />
      )}
    </>
  )
}
