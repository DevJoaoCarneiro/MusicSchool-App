import { sessionStorageKey } from '@/config/storage'
import type { ApiResponse, ApiStatus } from '@/types/api'
import type { AuthSession } from '@/types/entities'

type QueryValue = string | number | undefined | null
type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'

export const unauthorizedEvent = 'musicschool:unauthorized'

const baseUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

const fallbackMessages: Record<number, { status: ApiStatus; message: string }> = {
  401: { status: 'unauthorized', message: 'Sua sessão expirou. Entre novamente.' },
  403: { status: 'forbidden', message: 'Você não tem permissão para esta ação.' },
  404: { status: 'not_found', message: 'Recurso não encontrado.' },
  429: { status: 'error', message: 'Muitas tentativas. Aguarde um pouco e tente novamente.' },
}

function readToken() {
  try {
    const raw = sessionStorage.getItem(sessionStorageKey)
    return raw ? (JSON.parse(raw) as AuthSession).token : null
  } catch {
    return null
  }
}

function buildUrl(path: string, query?: Record<string, QueryValue>) {
  const params = new URLSearchParams()

  for (const [key, value] of Object.entries(query ?? {})) {
    if (value !== undefined && value !== null && value !== '') params.set(key, String(value))
  }

  const search = params.toString()
  return `${baseUrl}/api${path}${search ? `?${search}` : ''}`
}

function withoutEmpty(body: unknown): unknown {
  if (Array.isArray(body)) return body.map(withoutEmpty)

  if (body && typeof body === 'object') {
    return Object.fromEntries(
      Object.entries(body)
        .filter(([, value]) => value !== '' && value !== undefined && !(typeof value === 'number' && Number.isNaN(value)))
        .map(([key, value]) => [key, withoutEmpty(value)]),
    )
  }

  return body
}

function isEnvelope(value: unknown): value is { message?: string; status?: string; data?: unknown } {
  return Boolean(value) && typeof value === 'object' && ('status' in (value as object) || 'message' in (value as object))
}

async function request<T>(
  method: HttpMethod,
  path: string,
  options: { body?: unknown; query?: Record<string, QueryValue> } = {},
): Promise<ApiResponse<T>> {
  const token = readToken()
  const headers: Record<string, string> = { Accept: 'application/json' }

  if (options.body !== undefined) headers['Content-Type'] = 'application/json'
  if (token) headers.Authorization = `Bearer ${token}`

  let response: Response

  try {
    response = await fetch(buildUrl(path, options.query), {
      method,
      headers,
      body: options.body !== undefined ? JSON.stringify(withoutEmpty(options.body)) : undefined,
    })
  } catch {
    return { status: 'error', message: 'Não foi possível conectar ao servidor.', data: null }
  }

  const payload: unknown = await response.json().catch(() => null)

  if (response.status === 401 && token && !isEnvelope(payload)) {
    window.dispatchEvent(new Event(unauthorizedEvent))
  }

  if (isEnvelope(payload)) {
    return {
      status: (payload.status as ApiStatus) ?? (response.ok ? 'Success' : 'error'),
      message: payload.message ?? '',
      data: (payload.data ?? null) as T | null,
    }
  }

  const fallback = fallbackMessages[response.status]

  if (fallback) return { ...fallback, data: null }

  return response.ok
    ? { status: 'Success', message: '', data: payload as T | null }
    : { status: 'error', message: 'Ocorreu um erro inesperado.', data: null }
}

export const http = {
  get: <T>(path: string, query?: Record<string, QueryValue>) => request<T>('GET', path, { query }),
  post: <T>(path: string, body?: unknown) => request<T>('POST', path, { body: body ?? {} }),
  put: <T>(path: string, body: unknown) => request<T>('PUT', path, { body }),
  patch: <T>(path: string, body?: unknown) => request<T>('PATCH', path, { body: body ?? {} }),
  delete: <T>(path: string) => request<T>('DELETE', path),
}
