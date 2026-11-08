import { useCallback, useEffect, useState } from 'react'
import type { ApiResponse } from '@/types/api'

interface QueryResult<T> {
  fetcher: () => Promise<ApiResponse<T>>
  version: number
  data: T | null
  error: string | null
}

export function useApiQuery<T>(fetcher: () => Promise<ApiResponse<T>>) {
  const [version, setVersion] = useState(0)
  const [result, setResult] = useState<QueryResult<T> | null>(null)

  useEffect(() => {
    let active = true

    fetcher()
      .then((response) => {
        if (!active) return

        const succeeded = response.status === 'Success'
        setResult({
          fetcher,
          version,
          data: succeeded ? response.data : null,
          error: succeeded ? null : response.message,
        })
      })
      .catch(() => {
        if (active) {
          setResult({ fetcher, version, data: null, error: 'Não foi possível carregar os dados' })
        }
      })

    return () => {
      active = false
    }
  }, [fetcher, version])

  const reload = useCallback(() => setVersion((current) => current + 1), [])
  const loading = result === null || result.fetcher !== fetcher || result.version !== version

  return {
    data: result?.data ?? null,
    error: loading ? null : (result?.error ?? null),
    loading,
    reload,
  }
}
