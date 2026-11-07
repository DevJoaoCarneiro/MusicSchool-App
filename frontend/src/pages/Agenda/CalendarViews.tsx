import type { LessonListItem } from '@/types/entities'
import { cn } from '@/utils/cn'
import { addDays, isSameDay, toIsoDate } from '@/utils/dates'
import { formatWeekday } from '@/utils/format'
import styles from './Agenda.module.css'
import { LessonCard } from './LessonCard'

interface ViewProps {
  start: Date
  today: Date
  lessons: LessonListItem[]
  onSelect: (lesson: LessonListItem) => void
}

function groupByDay(lessons: LessonListItem[]) {
  const map = new Map<string, LessonListItem[]>()

  for (const lesson of lessons) {
    const key = lesson.startsAt.slice(0, 10)
    map.set(key, [...(map.get(key) ?? []), lesson])
  }

  return map
}

export function WeekView({ start, today, lessons, onSelect }: ViewProps) {
  const byDay = groupByDay(lessons)
  const days = Array.from({ length: 7 }, (_, index) => addDays(start, index))

  return (
    <div className={styles.week}>
      {days.map((day) => {
        const dayLessons = byDay.get(toIsoDate(day)) ?? []
        return (
          <section key={toIsoDate(day)} className={styles.weekDay} aria-label={day.toLocaleDateString('pt-BR')}>
            <header className={cn(styles.weekDayHeader, isSameDay(day, today) && styles.today)}>
              <span className={styles.weekdayName}>{formatWeekday(day)}</span>
              <span className={styles.weekdayNumber}>{day.getDate()}</span>
            </header>
            <div className={styles.weekDayBody}>
              {dayLessons.length === 0 ? (
                <span className={styles.noLessons}>Sem aulas</span>
              ) : (
                dayLessons.map((lesson) => <LessonCard key={lesson.id} lesson={lesson} onSelect={onSelect} />)
              )}
            </div>
          </section>
        )
      })}
    </div>
  )
}

interface MonthViewProps extends ViewProps {
  month: number
  onShowDay: (day: Date) => void
}

const maxChipsPerDay = 3
const weekdayHeaders = ['seg', 'ter', 'qua', 'qui', 'sex', 'sáb', 'dom']

export function MonthView({ start, today, month, lessons, onSelect, onShowDay }: MonthViewProps) {
  const byDay = groupByDay(lessons)
  const days = Array.from({ length: 42 }, (_, index) => addDays(start, index))

  return (
    <div className={styles.month}>
      {weekdayHeaders.map((name) => (
        <span key={name} className={styles.monthHeader}>
          {name}
        </span>
      ))}
      {days.map((day) => {
        const dayLessons = byDay.get(toIsoDate(day)) ?? []
        const hidden = dayLessons.length - maxChipsPerDay
        return (
          <div
            key={toIsoDate(day)}
            className={cn(styles.monthCell, day.getMonth() !== month && styles.outside)}
          >
            <button
              type="button"
              className={cn(styles.monthDay, isSameDay(day, today) && styles.todayBadge)}
              aria-label={`Ver semana de ${day.toLocaleDateString('pt-BR')}`}
              onClick={() => onShowDay(day)}
            >
              {day.getDate()}
            </button>
            {dayLessons.slice(0, maxChipsPerDay).map((lesson) => (
              <LessonCard key={lesson.id} lesson={lesson} compact onSelect={onSelect} />
            ))}
            {hidden > 0 && (
              <button type="button" className={styles.more} onClick={() => onShowDay(day)}>
                +{hidden} mais
              </button>
            )}
          </div>
        )
      })}
    </div>
  )
}
