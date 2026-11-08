import { useCallback, useState } from 'react'
import type { ApiResponse, PagedResult, PageParams } from '@/types/api'
import { useApiQuery } from './useApiQuery'
import { useDebouncedValue } from './useDebouncedValue'

export interface PagedListParams extends PageParams {
  name?: string
}

export function usePagedList<T>(
  fetchPage: (params: PagedListParams) => Promise<ApiResponse<PagedResult<T>>>,
  pageSize = 10,
) {
  const [page, setPage] = useState(1)
  const [search, setSearchValue] = useState('')
  const name = useDebouncedValue(search)

  const fetcher = useCallback(
    () => fetchPage({ name: name || undefined, page, pageSize }),
    [fetchPage, name, page, pageSize],
  )

  const query = useApiQuery(fetcher)

  const setSearch = useCallback((value: string) => {
    setSearchValue(value)
    setPage(1)
  }, [])

  return {
    ...query,
    items: query.data?.items ?? [],
    total: query.data?.total ?? 0,
    page,
    pageSize,
    setPage,
    search,
    setSearch,
  }
}
