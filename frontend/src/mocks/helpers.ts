import type { ApiResponse, ApiStatus, PagedResult, PageParams } from '@/types/api'

const latencyMs = 300

export function delay(ms = latencyMs) {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

export async function respond<T>(
  data: T | null,
  message = 'Success',
  status: ApiStatus = 'Success',
): Promise<ApiResponse<T>> {
  await delay()
  return { message, status, data }
}

export function fail<T>(status: ApiStatus, message: string): Promise<ApiResponse<T>> {
  return respond<T>(null, message, status)
}

export function paginate<T>(items: T[], { page, pageSize }: PageParams): PagedResult<T> {
  const start = (page - 1) * pageSize
  return {
    items: items.slice(start, start + pageSize),
    page,
    pageSize,
    total: items.length,
  }
}

export function matchesName(name: string, search?: string) {
  if (!search?.trim()) return true
  return name.toLocaleLowerCase('pt-BR').includes(search.trim().toLocaleLowerCase('pt-BR'))
}

export function newId(prefix: string) {
  return `${prefix}-${crypto.randomUUID()}`
}

export function emailTaken(list: Array<{ email: string; id: string }>, email: string, exceptId?: string) {
  const normalized = email.trim().toLowerCase()
  return list.some((item) => item.email.toLowerCase() === normalized && item.id !== exceptId)
}
