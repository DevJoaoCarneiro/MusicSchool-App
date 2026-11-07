import { enrollments, instruments, lessons, students } from '@/mocks/db'
import { fail, newId, paginate, respond } from '@/mocks/helpers'
import { currentTeacherScope } from '@/mocks/session'
import type { ApiResponse, PagedResult, PageParams } from '@/types/api'
import type {
  CloseEnrollmentRequest,
  CloseEnrollmentResult,
  EnrollmentDetail,
  EnrollmentListItem,
  EnrollmentRequest,
  EnrollmentStatus,
} from '@/types/entities'
import { toIsoDate } from '@/utils/dates'
import type { MockEnrollment } from '@/mocks/db'

export interface EnrollmentListParams extends PageParams {
  studentId?: string
  instrumentId?: string
  status?: EnrollmentStatus
}

function nameOf(list: Array<{ id: string; name: string }>, id: string) {
  return list.find((item) => item.id === id)?.name ?? '—'
}

function toListItem(enrollment: MockEnrollment): EnrollmentListItem {
  return {
    id: enrollment.id,
    studentName: nameOf(students, enrollment.studentId),
    instrumentName: nameOf(instruments, enrollment.instrumentId),
    startDate: enrollment.startDate,
    status: enrollment.status,
  }
}

function toDetail(enrollment: MockEnrollment): EnrollmentDetail {
  return {
    id: enrollment.id,
    student: { id: enrollment.studentId, name: nameOf(students, enrollment.studentId) },
    instrument: { id: enrollment.instrumentId, name: nameOf(instruments, enrollment.instrumentId) },
    startDate: enrollment.startDate,
    monthlyFee: enrollment.monthlyFee,
    dueDay: enrollment.dueDay,
    status: enrollment.status,
  }
}

function visibleToCaller(enrollment: MockEnrollment) {
  const scope = currentTeacherScope()
  if (!scope.scoped) return true
  return lessons.some((lesson) => lesson.enrollmentId === enrollment.id && lesson.teacherId === scope.teacherId)
}

export const enrollmentService = {
  async list({
    studentId,
    instrumentId,
    status,
    ...page
  }: EnrollmentListParams): Promise<ApiResponse<PagedResult<EnrollmentListItem>>> {
    const filtered = enrollments
      .filter((enrollment) => !studentId || enrollment.studentId === studentId)
      .filter((enrollment) => !instrumentId || enrollment.instrumentId === instrumentId)
      .filter((enrollment) => !status || enrollment.status === status)
      .filter(visibleToCaller)
      .sort((a, b) => a.startDate.localeCompare(b.startDate))
      .map(toListItem)

    return respond(paginate(filtered, page))
  },

  async getById(id: string): Promise<ApiResponse<EnrollmentDetail>> {
    const enrollment = enrollments.find((item) => item.id === id)

    if (!enrollment || !visibleToCaller(enrollment)) {
      return fail('not_found', 'Matrícula não encontrada')
    }

    return respond(toDetail(enrollment))
  },

  async create(request: EnrollmentRequest): Promise<ApiResponse<EnrollmentDetail>> {
    if (!request.studentId || !request.instrumentId) {
      return fail('invalid_argument', 'Aluno e instrumento são obrigatórios')
    }

    if (!request.startDate) return fail('invalid_argument', 'Data de início inválida')
    if (!(request.monthlyFee > 0)) return fail('invalid_argument', 'Mensalidade deve ser maior que zero')

    if (!Number.isInteger(request.dueDay) || request.dueDay < 1 || request.dueDay > 28) {
      return fail('invalid_argument', 'Dia de vencimento deve estar entre 1 e 28')
    }

    const duplicated = enrollments.some(
      (item) =>
        item.studentId === request.studentId &&
        item.instrumentId === request.instrumentId &&
        item.status === 'Active',
    )

    if (duplicated) return fail('conflict', 'Aluno já possui matrícula ativa neste instrumento')

    const enrollment: MockEnrollment = {
      id: newId('e'),
      ...request,
      status: 'Active',
      endDate: null,
      reason: null,
    }

    enrollments.push(enrollment)
    return respond(toDetail(enrollment), 'Matrícula criada com sucesso')
  },

  async close(id: string, request: CloseEnrollmentRequest): Promise<ApiResponse<CloseEnrollmentResult>> {
    const enrollment = enrollments.find((item) => item.id === id)

    if (!enrollment) return fail('not_found', 'Matrícula não encontrada')
    if (enrollment.status === 'Closed') return fail('conflict', 'Matrícula já está encerrada')

    const endDate = request.endDate || toIsoDate(new Date())

    if (endDate < enrollment.startDate) {
      return fail('invalid_argument', 'Data de término não pode ser anterior ao início')
    }

    const hasLessonsAfter = lessons.some(
      (lesson) =>
        lesson.enrollmentId === id && lesson.status === 'Scheduled' && lesson.startsAt.slice(0, 10) > endDate,
    )

    if (hasLessonsAfter) {
      return fail('conflict', 'Existem aulas agendadas após a data de término')
    }

    enrollment.status = 'Closed'
    enrollment.endDate = endDate
    enrollment.reason = request.reason
    return respond({ id, status: 'Closed', endDate }, 'Matrícula encerrada com sucesso')
  },
}
