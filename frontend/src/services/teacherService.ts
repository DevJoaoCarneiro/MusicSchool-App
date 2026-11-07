import type { PagedResult, PageParams } from '@/types/api'
import type { TeacherDetail, TeacherListItem, TeacherRequest } from '@/types/entities'
import { http } from './http'

export interface TeacherListParams extends PageParams {
  name?: string
}

export const teacherService = {
  list: ({ name, page, pageSize }: TeacherListParams) =>
    http.get<PagedResult<TeacherListItem>>('/teachers', { name, page, pageSize }),

  getById: (id: string) => http.get<TeacherDetail>(`/teachers/${id}`),

  create: (request: TeacherRequest) => http.post<TeacherDetail>('/teachers', request),

  update: (id: string, request: TeacherRequest) => http.put<TeacherDetail>(`/teachers/${id}`, request),

  remove: (id: string) => http.delete<null>(`/teachers/${id}`),
}
