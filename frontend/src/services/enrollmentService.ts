import type { PagedResult, PageParams } from '@/types/api'
import type {
  CloseEnrollmentRequest,
  CloseEnrollmentResult,
  EnrollmentDetail,
  EnrollmentListItem,
  EnrollmentRequest,
  EnrollmentStatus,
} from '@/types/entities'
import { http } from './http'

export interface EnrollmentListParams extends PageParams {
  studentId?: string
  instrumentId?: string
  status?: EnrollmentStatus
}

export const enrollmentService = {
  list: (params: EnrollmentListParams) => http.get<PagedResult<EnrollmentListItem>>('/enrollments', { ...params }),

  getById: (id: string) => http.get<EnrollmentDetail>(`/enrollments/${id}`),

  create: (request: EnrollmentRequest) => http.post<EnrollmentDetail>('/enrollments', request),

  close: (id: string, request: CloseEnrollmentRequest) =>
    http.patch<CloseEnrollmentResult>(`/enrollments/${id}/close`, request),
}
