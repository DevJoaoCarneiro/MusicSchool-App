import { lessons, teachers } from '@/mocks/db'
import { emailTaken, fail, matchesName, newId, paginate, respond } from '@/mocks/helpers'
import type { ApiResponse, PagedResult, PageParams } from '@/types/api'
import type { TeacherDetail, TeacherListItem, TeacherRequest } from '@/types/entities'

export interface TeacherListParams extends PageParams {
  name?: string
}

function toListItem({ id, name, email, specialties }: TeacherDetail): TeacherListItem {
  return { id, name, email, specialties }
}

function cpfTaken(cpf: string, exceptId?: string) {
  return teachers.some((teacher) => teacher.cpf === cpf && teacher.id !== exceptId)
}

function normalize(request: TeacherRequest): TeacherRequest {
  return { ...request, name: request.name.trim(), email: request.email.trim().toLowerCase() }
}

export const teacherService = {
  async list({ name, ...page }: TeacherListParams): Promise<ApiResponse<PagedResult<TeacherListItem>>> {
    const filtered = teachers
      .filter((teacher) => matchesName(teacher.name, name))
      .sort((a, b) => a.name.localeCompare(b.name, 'pt-BR'))
      .map(toListItem)

    return respond(paginate(filtered, page))
  },

  async getById(id: string): Promise<ApiResponse<TeacherDetail>> {
    const teacher = teachers.find((item) => item.id === id)
    return teacher ? respond(structuredClone(teacher)) : fail('not_found', 'Professor não encontrado')
  },

  async create(request: TeacherRequest): Promise<ApiResponse<TeacherDetail>> {
    if (request.specialties.length === 0) return fail('invalid_argument', 'Informe ao menos uma especialidade')
    if (emailTaken(teachers, request.email)) return fail('conflict', 'E-mail já cadastrado')
    if (cpfTaken(request.cpf)) return fail('conflict', 'CPF já cadastrado')

    const teacher: TeacherDetail = { id: newId('t'), ...normalize(request) }
    teachers.push(teacher)
    return respond(structuredClone(teacher), 'Professor cadastrado com sucesso')
  },

  async update(id: string, request: TeacherRequest): Promise<ApiResponse<TeacherDetail>> {
    const teacher = teachers.find((item) => item.id === id)

    if (!teacher) return fail('not_found', 'Professor não encontrado')
    if (request.specialties.length === 0) return fail('invalid_argument', 'Informe ao menos uma especialidade')
    if (emailTaken(teachers, request.email, id)) return fail('conflict', 'E-mail já cadastrado')
    if (cpfTaken(request.cpf, id)) return fail('conflict', 'CPF já cadastrado')

    Object.assign(teacher, normalize(request))
    return respond(structuredClone(teacher), 'Professor atualizado com sucesso')
  },

  async remove(id: string): Promise<ApiResponse<null>> {
    const index = teachers.findIndex((item) => item.id === id)

    if (index === -1) return fail('not_found', 'Professor não encontrado')

    if (lessons.some((lesson) => lesson.teacherId === id && lesson.status === 'Scheduled')) {
      return fail('conflict', 'Não é possível excluir um professor com aulas agendadas')
    }

    teachers.splice(index, 1)
    return respond(null, 'Professor excluído com sucesso')
  },
}
