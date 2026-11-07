import type { PagedResult, PageParams } from '@/types/api'
import type { StudentDetail, StudentListItem, StudentRequest } from '@/types/entities'
import { http } from './http'

export interface StudentListParams extends PageParams {
  name?: string
}

export const studentService = {
  list: ({ name, page, pageSize }: StudentListParams) =>
    http.get<PagedResult<StudentListItem>>('/students', { name, page, pageSize }),

  getById: (id: string) => http.get<StudentDetail>(`/students/${id}`),

  create: (request: StudentRequest) => http.post<StudentDetail>('/students', request),

  update: (id: string, request: StudentRequest) => http.put<StudentDetail>(`/students/${id}`, request),

  remove: (id: string) => http.delete<null>(`/students/${id}`),
}
