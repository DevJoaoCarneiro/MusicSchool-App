import type { PagedResult, PageParams } from '@/types/api'
import type { UserListItem, UserRequest } from '@/types/entities'
import { http } from './http'

export const userService = {
  list: ({ page, pageSize }: PageParams) => http.get<PagedResult<UserListItem>>('/users', { page, pageSize }),

  create: (request: UserRequest) => http.post<UserListItem>('/users', request),
}
