import { dueDateForMonth, enrollments, instruments, students, tuitions, type MockTuition } from '@/mocks/db'
import { fail, newId, paginate, respond } from '@/mocks/helpers'
import type { ApiResponse, PagedResult, PageParams } from '@/types/api'
import type {
  GenerateTuitionsResult,
  RegisterPaymentRequest,
  TuitionListItem,
  TuitionStatus,
} from '@/types/entities'
import { toIsoDate, toIsoMonth } from '@/utils/dates'

export interface TuitionListParams extends PageParams {
  referenceMonth?: string
  status?: TuitionStatus
  studentId?: string
}

function nameOf(list: Array<{ id: string; name: string }>, id: string | undefined) {
  return list.find((item) => item.id === id)?.name ?? '—'
}

function toListItem(tuition: MockTuition): TuitionListItem {
  const enrollment = enrollments.find((item) => item.id === tuition.enrollmentId)
  return {
    ...tuition,
    studentName: nameOf(students, enrollment?.studentId),
    instrumentName: nameOf(instruments, enrollment?.instrumentId),
  }
}

function monthBounds(referenceMonth: string) {
  const [year, month] = referenceMonth.split('-').map(Number)
  const lastDay = new Date(year, month, 0).getDate()
  return { start: `${referenceMonth}-01`, end: `${referenceMonth}-${String(lastDay).padStart(2, '0')}` }
}

export const tuitionService = {
  async list({
    referenceMonth,
    status,
    studentId,
    ...page
  }: TuitionListParams): Promise<ApiResponse<PagedResult<TuitionListItem>>> {
    if (referenceMonth && !/^\d{4}-\d{2}$/.test(referenceMonth)) {
      return fail('invalid_argument', 'Mês de referência deve estar no formato yyyy-MM')
    }

    const filtered = tuitions
      .filter((tuition) => !referenceMonth || tuition.referenceMonth === referenceMonth)
      .filter((tuition) => !status || tuition.status === status)
      .filter(
        (tuition) =>
          !studentId || enrollments.find((item) => item.id === tuition.enrollmentId)?.studentId === studentId,
      )
      .sort((a, b) => a.dueDate.localeCompare(b.dueDate))
      .map(toListItem)

    return respond(paginate(filtered, page))
  },

  async generate(referenceMonth: string): Promise<ApiResponse<GenerateTuitionsResult>> {
    if (!/^\d{4}-(0[1-9]|1[0-2])$/.test(referenceMonth)) {
      return fail('invalid_argument', 'Mês de referência deve estar no formato yyyy-MM')
    }

    const nextMonth = toIsoMonth(new Date(new Date().getFullYear(), new Date().getMonth() + 1, 1))

    if (referenceMonth > nextMonth) {
      return fail('invalid_argument', `Mês de referência deve ser no máximo ${nextMonth}`)
    }

    const { start, end } = monthBounds(referenceMonth)
    const billable = enrollments.filter(
      (enrollment) =>
        enrollment.startDate <= end &&
        (enrollment.status === 'Active' || (enrollment.endDate !== null && enrollment.endDate >= start)),
    )

    let created = 0
    let skipped = 0

    for (const enrollment of billable) {
      const exists = tuitions.some(
        (tuition) => tuition.enrollmentId === enrollment.id && tuition.referenceMonth === referenceMonth,
      )

      if (exists) {
        skipped += 1
        continue
      }

      tuitions.push({
        id: newId('tu'),
        enrollmentId: enrollment.id,
        referenceMonth,
        amount: enrollment.monthlyFee,
        dueDate: dueDateForMonth(referenceMonth, enrollment.dueDay),
        status: 'Pending',
        paidAt: null,
        paidAmount: null,
        method: null,
      })
      created += 1
    }

    return respond({ referenceMonth, created, skipped }, 'Mensalidades geradas com sucesso')
  },

  async registerPayment(id: string, request: RegisterPaymentRequest): Promise<ApiResponse<TuitionListItem>> {
    const tuition = tuitions.find((item) => item.id === id)

    if (!tuition) return fail('not_found', 'Mensalidade não encontrada')
    if (tuition.status === 'Paid') return fail('conflict', 'Mensalidade já está paga')

    const paidAt = request.paidAt || toIsoDate(new Date())
    const amount = request.amount ?? tuition.amount

    if (paidAt > toIsoDate(new Date())) return fail('invalid_argument', 'Data de pagamento não pode ser futura')
    if (!(amount > 0)) return fail('invalid_argument', 'Valor deve ser maior que zero')
    if (!request.method) return fail('invalid_argument', 'Forma de pagamento é obrigatória')

    tuition.status = 'Paid'
    tuition.paidAt = paidAt
    tuition.paidAmount = amount
    tuition.method = request.method
    return respond(toListItem(tuition), 'Pagamento registrado com sucesso')
  },
}
