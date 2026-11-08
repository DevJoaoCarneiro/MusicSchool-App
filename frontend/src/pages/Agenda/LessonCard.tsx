import { lessonStatus } from '@/config/labels'
import type { LessonListItem } from '@/types/entities'
import { cn } from '@/utils/cn'
import { formatTime } from '@/utils/format'
import styles from './Agenda.module.css'

interface LessonCardProps {
  lesson: LessonListItem
  compact?: boolean
  onSelect: (lesson: LessonListItem) => void
}

export function LessonCard({ lesson, compact = false, onSelect }: LessonCardProps) {
  const status = lessonStatus[lesson.status]
  const time = `${formatTime(lesson.startsAt)} – ${formatTime(lesson.endsAt)}`

  if (compact) {
    return (
      <button
        type="button"
        className={cn(styles.chip, styles[`tone-${status.tone}`])}
        title={`${time} · ${lesson.studentName} · ${status.label}`}
        onClick={() => onSelect(lesson)}
      >
        <span className={styles.chipTime}>{formatTime(lesson.startsAt)}</span>
        <span className={styles.chipText}>{lesson.studentName}</span>
      </button>
    )
  }

  return (
    <button
      type="button"
      className={cn(styles.card, styles[`tone-${status.tone}`])}
      aria-label={`${time}, ${lesson.studentName}, ${lesson.instrument}, ${status.label}`}
      onClick={() => onSelect(lesson)}
    >
      <span className={styles.cardTime}>{time}</span>
      <span className={styles.cardStudent}>{lesson.studentName}</span>
      <span className={styles.cardMeta}>{lesson.instrument}</span>
      <span className={styles.cardMeta}>{lesson.teacherName}</span>
      <span className={styles.cardFooter}>
        <span>{lesson.room}</span>
        <span className={styles.cardStatus}>{status.label}</span>
      </span>
    </button>
  )
}
