import type { PagedResult, PageParams } from '@/types/api'
import type {
  GenerateTuitionsResult,
  RegisterPaymentRequest,
  TuitionListItem,
  TuitionStatus,
} from '@/types/entities'
import { http } from './http'

export interface TuitionListParams extends PageParams {
  referenceMonth?: string
  status?: TuitionStatus
  studentId?: string
}

export const tuitionService = {
  list: (params: TuitionListParams) => http.get<PagedResult<TuitionListItem>>('/tuitions', { ...params }),

  generate: (referenceMonth: string) => http.post<GenerateTuitionsResult>('/tuitions/generate', { referenceMonth }),

  registerPayment: (id: string, request: RegisterPaymentRequest) =>
    http.patch<unknown>(`/tuitions/${id}/payment`, request),
}
