import type {
  CancelLessonRequest,
  LessonConflicts,
  LessonDetail,
  LessonListItem,
  LessonRequest,
  RecurringLessonRequest,
  RecurringLessonResult,
  RescheduleLessonRequest,
} from '@/types/entities'
import { http } from './http'

export interface LessonRangeParams {
  from: string
  to: string
  teacherId?: string
  studentId?: string
}

export const lessonService = {
  listByRange: (params: LessonRangeParams) => http.get<LessonListItem[]>('/lessons', { ...params }),

  getById: (id: string) => http.get<LessonDetail>(`/lessons/${id}`),

  create: (request: LessonRequest) => http.post<LessonDetail>('/lessons', request),

  createRecurring: (request: RecurringLessonRequest) =>
    http.post<RecurringLessonResult | LessonConflicts>('/lessons/recurring', request),

  cancel: (id: string, request: CancelLessonRequest) => http.patch<LessonDetail>(`/lessons/${id}/cancel`, request),

  complete: (id: string) => http.patch<LessonDetail>(`/lessons/${id}/complete`),

  reschedule: (id: string, request: RescheduleLessonRequest) =>
    http.patch<LessonDetail>(`/lessons/${id}/reschedule`, request),

  cancelSeries: (seriesId: string, request: CancelLessonRequest) =>
    http.patch<unknown>(`/lessons/series/${seriesId}/cancel`, request),
}
