import { useCallback, useState } from 'react'
import type { ApiResponse, PagedResult, PageParams } from '@/types/api'
import { useApiQuery } from './useApiQuery'

export function usePagedQuery<T, F extends object>(
  fetchPage: (params: F & PageParams) => Promise<ApiResponse<PagedResult<T>>>,
  initialFilters: F,
  pageSize = 10,
) {
  const [page, setPage] = useState(1)
  const [filters, setFiltersState] = useState<F>(initialFilters)

  const fetcher = useCallback(() => fetchPage({ ...filters, page, pageSize }), [fetchPage, filters, page, pageSize])
  const query = useApiQuery(fetcher)

  const setFilter = useCallback(<K extends keyof F>(key: K, value: F[K]) => {
    setFiltersState((current) => ({ ...current, [key]: value }))
    setPage(1)
  }, [])

  return {
    ...query,
    items: query.data?.items ?? [],
    total: query.data?.total ?? 0,
    page,
    pageSize,
    setPage,
    filters,
    setFilter,
  }
}
