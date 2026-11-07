export type ApiStatus =
  | 'Success'
  | 'invalid_argument'
  | 'unauthorized'
  | 'forbidden'
  | 'not_found'
  | 'conflict'
  | 'error'

export interface ApiResponse<T> {
  message: string
  status: ApiStatus
  data: T | null
}

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}
