import { instruments } from '@/mocks/db'
import { fail, newId, respond } from '@/mocks/helpers'
import type { ApiResponse } from '@/types/api'
import type { Instrument, InstrumentRequest } from '@/types/entities'

function nameTaken(name: string, exceptId?: string) {
  const normalized = name.trim().toLocaleLowerCase('pt-BR')
  return instruments.some(
    (item) => item.name.toLocaleLowerCase('pt-BR') === normalized && item.id !== exceptId,
  )
}

export const instrumentService = {
  async list(): Promise<ApiResponse<Instrument[]>> {
    return respond([...instruments])
  },

  async create(request: InstrumentRequest): Promise<ApiResponse<Instrument>> {
    if (nameTaken(request.name)) {
      return fail('conflict', 'Instrumento já cadastrado')
    }

    const instrument = { id: newId('i'), ...request }
    instruments.push(instrument)
    return respond(instrument, 'Instrumento criado com sucesso')
  },

  async update(id: string, request: InstrumentRequest): Promise<ApiResponse<Instrument>> {
    const instrument = instruments.find((item) => item.id === id)

    if (!instrument) {
      return fail('not_found', 'Instrumento não encontrado')
    }

    if (nameTaken(request.name, id)) {
      return fail('conflict', 'Instrumento já cadastrado')
    }

    Object.assign(instrument, request)
    return respond({ ...instrument }, 'Instrumento atualizado com sucesso')
  },

  async remove(id: string): Promise<ApiResponse<null>> {
    const index = instruments.findIndex((item) => item.id === id)

    if (index === -1) {
      return fail('not_found', 'Instrumento não encontrado')
    }

    instruments.splice(index, 1)
    return respond(null, 'Instrumento excluído com sucesso')
  },
}
