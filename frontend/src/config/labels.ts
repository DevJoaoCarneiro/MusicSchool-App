import type { BadgeTone } from '@/components/ui/Badge/Badge'
import type { EnrollmentStatus, LessonStatus, PaymentMethod, TuitionStatus, Weekday } from '@/types/entities'

interface StatusLabel {
  label: string
  tone: BadgeTone
}

export const enrollmentStatus: Record<EnrollmentStatus, StatusLabel> = {
  Active: { label: 'Ativa', tone: 'success' },
  Closed: { label: 'Encerrada', tone: 'neutral' },
}

export const lessonStatus: Record<LessonStatus, StatusLabel> = {
  Scheduled: { label: 'Agendada', tone: 'info' },
  Completed: { label: 'Concluída', tone: 'success' },
  Cancelled: { label: 'Cancelada', tone: 'danger' },
}

export const tuitionStatus: Record<TuitionStatus, StatusLabel> = {
  Pending: { label: 'Pendente', tone: 'warning' },
  Paid: { label: 'Paga', tone: 'success' },
}

export const paymentMethods: Record<PaymentMethod, string> = {
  Pix: 'Pix',
  Cash: 'Dinheiro',
  Card: 'Cartão',
}

export const weekdays: Array<{ value: Weekday; label: string }> = [
  { value: 'Monday', label: 'Segunda-feira' },
  { value: 'Tuesday', label: 'Terça-feira' },
  { value: 'Wednesday', label: 'Quarta-feira' },
  { value: 'Thursday', label: 'Quinta-feira' },
  { value: 'Friday', label: 'Sexta-feira' },
  { value: 'Saturday', label: 'Sábado' },
  { value: 'Sunday', label: 'Domingo' },
]

export function optionsFrom<T extends string>(map: Record<T, StatusLabel | string>) {
  return (Object.keys(map) as T[]).map((value) => {
    const entry = map[value]
    return { value, label: typeof entry === 'string' ? entry : entry.label }
  })
}
