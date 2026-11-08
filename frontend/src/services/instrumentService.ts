import type { Instrument, InstrumentRequest } from '@/types/entities'
import { http } from './http'

export const instrumentService = {
  list: () => http.get<Instrument[]>('/instruments'),

  create: (request: InstrumentRequest) => http.post<Instrument>('/instruments', request),

  update: (id: string, request: InstrumentRequest) => http.put<Instrument>(`/instruments/${id}`, request),

  remove: (id: string) => http.delete<null>(`/instruments/${id}`),
}
