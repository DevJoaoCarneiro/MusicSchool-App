import { enrollments, instruments, lessons, students, teachers, type MockLesson } from '@/mocks/db'
import { fail, newId, respond } from '@/mocks/helpers'
import { currentTeacherScope } from '@/mocks/session'
import type { ApiResponse } from '@/types/api'
import type {
  CancelLessonRequest,
  LessonConflicts,
  LessonDetail,
  LessonListItem,
  LessonRequest,
  RecurringLessonRequest,
  RecurringLessonResult,
  RescheduleLessonRequest,
  Weekday,
} from '@/types/entities'
import { addDays, addMinutes, combineDateTime, parseLocal, toIsoDate, toLocalIso } from '@/utils/dates'

export interface LessonRangeParams {
  from: string
  to: string
  teacherId?: string
  studentId?: string
}

const maxRangeDays = 42
const maxRecurringDays = 183
const weekdayIndex: Record<Weekday, number> = {
  Sunday: 0,
  Monday: 1,
  Tuesday: 2,
  Wednesday: 3,
  Thursday: 4,
  Friday: 5,
  Saturday: 6,
}

function nameOf(list: Array<{ id: string; name: string }>, id: string) {
  return list.find((item) => item.id === id)?.name ?? '—'
}

function enrollmentOf(lesson: MockLesson) {
  return enrollments.find((item) => item.id === lesson.enrollmentId)
}

function endsAt(lesson: MockLesson) {
  return toLocalIso(addMinutes(parseLocal(lesson.startsAt), lesson.durationMinutes))
}

function toListItem(lesson: MockLesson): LessonListItem {
  const enrollment = enrollmentOf(lesson)
  return {
    id: lesson.id,
    seriesId: lesson.seriesId,
    studentName: enrollment ? nameOf(students, enrollment.studentId) : '—',
    teacherName: nameOf(teachers, lesson.teacherId),
    instrument: enrollment ? nameOf(instruments, enrollment.instrumentId) : '—',
    startsAt: lesson.startsAt,
    endsAt: endsAt(lesson),
    room: lesson.room,
    status: lesson.status,
  }
}

function toDetail(lesson: MockLesson): LessonDetail {
  const enrollment = enrollmentOf(lesson)
  return {
    id: lesson.id,
    seriesId: lesson.seriesId,
    enrollmentId: lesson.enrollmentId,
    teacher: { id: lesson.teacherId, name: nameOf(teachers, lesson.teacherId) },
    student: {
      id: enrollment?.studentId ?? '',
      name: enrollment ? nameOf(students, enrollment.studentId) : '—',
    },
    instrument: enrollment ? nameOf(instruments, enrollment.instrumentId) : '—',
    startsAt: lesson.startsAt,
    endsAt: endsAt(lesson),
    room: lesson.room,
    status: lesson.status,
    reason: lesson.reason,
    cancelledAt: lesson.cancelledAt,
  }
}

function overlaps(startsAt: string, durationMinutes: number, teacherId: string, room: string, exceptId?: string) {
  const start = parseLocal(startsAt).getTime()
  const end = start + durationMinutes * 60_000
  const normalizedRoom = room.trim().toLowerCase()

  return lessons.some((lesson) => {
    if (lesson.id === exceptId || lesson.status !== 'Scheduled') return false
    if (lesson.teacherId !== teacherId && lesson.room.trim().toLowerCase() !== normalizedRoom) return false

    const lessonStart = parseLocal(lesson.startsAt).getTime()
    const lessonEnd = lessonStart + lesson.durationMinutes * 60_000
    return start < lessonEnd && lessonStart < end
  })
}

function validateBase(request: { enrollmentId: string; teacherId: string; durationMinutes: number; room: string }) {
  const enrollment = enrollments.find((item) => item.id === request.enrollmentId)

  if (!enrollment) return 'Matrícula não encontrada'
  if (enrollment.status !== 'Active') return 'A matrícula não está ativa'
  if (!teachers.some((teacher) => teacher.id === request.teacherId)) return 'Professor não encontrado'
  if (request.durationMinutes < 30 || request.durationMinutes > 120) return 'Duração deve estar entre 30 e 120 minutos'
  if (!request.room.trim()) return 'Sala é obrigatória'
  return null
}

function findLesson(id: string) {
  return lessons.find((item) => item.id === id)
}

export const lessonService = {
  async listByRange({ from, to, teacherId, studentId }: LessonRangeParams): Promise<ApiResponse<LessonListItem[]>> {
    const days = (parseLocal(to).getTime() - parseLocal(from).getTime()) / 86_400_000 + 1

    if (days > maxRangeDays) {
      return fail('invalid_argument', `O período não pode passar de ${maxRangeDays} dias`)
    }

    const scope = currentTeacherScope()

    if (scope.scoped && scope.teacherId === null) return respond<LessonListItem[]>([])

    const effectiveTeacherId = scope.scoped ? scope.teacherId : teacherId

    const items = lessons
      .filter((lesson) => {
        const day = lesson.startsAt.slice(0, 10)
        return day >= from && day <= to
      })
      .filter((lesson) => !effectiveTeacherId || lesson.teacherId === effectiveTeacherId)
      .filter((lesson) => !studentId || enrollmentOf(lesson)?.studentId === studentId)
      .sort((a, b) => a.startsAt.localeCompare(b.startsAt))
      .map(toListItem)

    return respond(items)
  },

  async getById(id: string): Promise<ApiResponse<LessonDetail>> {
    const lesson = findLesson(id)
    const scope = currentTeacherScope()

    if (!lesson || (scope.scoped && lesson.teacherId !== scope.teacherId)) {
      return fail('not_found', 'Aula não encontrada')
    }

    return respond(toDetail(lesson))
  },

  async create(request: LessonRequest): Promise<ApiResponse<LessonDetail>> {
    const error = validateBase(request)
    if (error) return fail('invalid_argument', error)

    if (parseLocal(request.startsAt).getTime() < Date.now()) {
      return fail('invalid_argument', 'Não é possível agendar aulas no passado')
    }

    if (overlaps(request.startsAt, request.durationMinutes, request.teacherId, request.room)) {
      return fail('conflict', 'Conflito de horário com outra aula do professor ou da sala')
    }

    const lesson: MockLesson = {
      id: newId('l'),
      seriesId: null,
      ...request,
      status: 'Scheduled',
      reason: null,
      cancelledAt: null,
    }

    lessons.push(lesson)
    return respond(toDetail(lesson), 'Aula agendada com sucesso')
  },

  async createRecurring(
    request: RecurringLessonRequest,
  ): Promise<ApiResponse<RecurringLessonResult | LessonConflicts>> {
    const error = validateBase(request)
    if (error) return fail('invalid_argument', error)

    if (!request.startDate || !request.endDate || request.endDate < request.startDate) {
      return fail('invalid_argument', 'Período inválido')
    }

    const days = (parseLocal(request.endDate).getTime() - parseLocal(request.startDate).getTime()) / 86_400_000

    if (days > maxRecurringDays) {
      return fail('invalid_argument', 'O período não pode passar de 6 meses')
    }

    if (!/^\d{2}:\d{2}$/.test(request.time)) return fail('invalid_argument', 'Horário inválido')

    const first = parseLocal(request.startDate)
    const offset = (weekdayIndex[request.weekday] - first.getDay() + 7) % 7
    const slots: string[] = []

    for (let date = addDays(first, offset); toIsoDate(date) <= request.endDate; date = addDays(date, 7)) {
      slots.push(combineDateTime(toIsoDate(date), request.time))
    }

    if (slots.length === 0) return fail('invalid_argument', 'Nenhuma data no período informado')

    const conflictingDates = slots
      .filter((slot) => overlaps(slot, request.durationMinutes, request.teacherId, request.room))
      .map((slot) => slot.slice(0, 10))

    if (conflictingDates.length > 0) {
      return respond<LessonConflicts>({ conflictingDates }, 'Conflitos encontrados nas aulas recorrentes', 'conflict')
    }

    const seriesId = newId('ls')

    for (const slot of slots) {
      lessons.push({
        id: newId('l'),
        seriesId,
        enrollmentId: request.enrollmentId,
        teacherId: request.teacherId,
        startsAt: slot,
        durationMinutes: request.durationMinutes,
        room: request.room,
        status: 'Scheduled',
        reason: null,
        cancelledAt: null,
      })
    }

    return respond<RecurringLessonResult>(
      { seriesId, lessonsCreated: slots.length, firstLesson: slots[0], lastLesson: slots[slots.length - 1] },
      'Aulas recorrentes agendadas com sucesso',
    )
  },

  async cancel(id: string, { reason }: CancelLessonRequest): Promise<ApiResponse<LessonDetail>> {
    const lesson = findLesson(id)

    if (!lesson) return fail('not_found', 'Aula não encontrada')
    if (lesson.status !== 'Scheduled') return fail('conflict', 'Somente aulas agendadas podem ser canceladas')
    if (!reason.trim()) return fail('invalid_argument', 'Motivo do cancelamento é obrigatório')

    lesson.status = 'Cancelled'
    lesson.reason = reason.trim()
    lesson.cancelledAt = toLocalIso(new Date())
    return respond(toDetail(lesson), 'Aula cancelada com sucesso')
  },

  async complete(id: string): Promise<ApiResponse<LessonDetail>> {
    const lesson = findLesson(id)

    if (!lesson) return fail('not_found', 'Aula não encontrada')
    if (lesson.status !== 'Scheduled') return fail('conflict', 'Somente aulas agendadas podem ser concluídas')

    if (parseLocal(lesson.startsAt).getTime() > Date.now()) {
      return fail('invalid_argument', 'Não é possível concluir uma aula que ainda não começou')
    }

    lesson.status = 'Completed'
    return respond(toDetail(lesson), 'Aula concluída com sucesso')
  },

  async reschedule(id: string, request: RescheduleLessonRequest): Promise<ApiResponse<LessonDetail>> {
    const lesson = findLesson(id)

    if (!lesson) return fail('not_found', 'Aula não encontrada')
    if (lesson.status !== 'Scheduled') return fail('conflict', 'Somente aulas agendadas podem ser reagendadas')
    if (!request.reason.trim()) return fail('invalid_argument', 'Motivo é obrigatório')
    if (!request.room.trim()) return fail('invalid_argument', 'Sala é obrigatória')

    if (parseLocal(request.newStartsAt).getTime() < Date.now()) {
      return fail('invalid_argument', 'Não é possível reagendar para o passado')
    }

    if (overlaps(request.newStartsAt, lesson.durationMinutes, lesson.teacherId, request.room, lesson.id)) {
      return fail('conflict', 'Conflito de horário com outra aula do professor ou da sala')
    }

    lesson.startsAt = request.newStartsAt
    lesson.room = request.room.trim()
    lesson.reason = request.reason.trim()
    return respond(toDetail(lesson), 'Aula reagendada com sucesso')
  },

  async cancelSeries(seriesId: string, { reason }: CancelLessonRequest): Promise<ApiResponse<{ cancelled: number }>> {
    if (!reason.trim()) return fail('invalid_argument', 'Motivo do cancelamento é obrigatório')

    const pending = lessons.filter((lesson) => lesson.seriesId === seriesId && lesson.status === 'Scheduled')

    if (pending.length === 0) return fail('not_found', 'Nenhuma aula agendada nesta série')

    const now = toLocalIso(new Date())

    for (const lesson of pending) {
      lesson.status = 'Cancelled'
      lesson.reason = reason.trim()
      lesson.cancelledAt = now
    }

    return respond({ cancelled: pending.length }, `${pending.length} aulas da série canceladas`)
  },
}
