import { enrollments, students } from '@/mocks/db'
import { emailTaken, fail, matchesName, newId, paginate, respond } from '@/mocks/helpers'
import type { ApiResponse, PagedResult, PageParams } from '@/types/api'
import type { StudentDetail, StudentListItem, StudentRequest } from '@/types/entities'

export interface StudentListParams extends PageParams {
  name?: string
}

function toListItem({ id, name, email, phone, birthDate }: StudentDetail): StudentListItem {
  return { id, name, email, phone, birthDate }
}

function cpfTaken(cpf: string, exceptId?: string) {
  return students.some((student) => student.cpf === cpf && student.id !== exceptId)
}

function normalize(request: StudentRequest): StudentRequest {
  return { ...request, name: request.name.trim(), email: request.email.trim().toLowerCase() }
}

export const studentService = {
  async list({ name, ...page }: StudentListParams): Promise<ApiResponse<PagedResult<StudentListItem>>> {
    const filtered = students
      .filter((student) => matchesName(student.name, name))
      .sort((a, b) => a.name.localeCompare(b.name, 'pt-BR'))
      .map(toListItem)

    return respond(paginate(filtered, page))
  },

  async getById(id: string): Promise<ApiResponse<StudentDetail>> {
    const student = students.find((item) => item.id === id)
    return student ? respond(structuredClone(student)) : fail('not_found', 'Aluno não encontrado')
  },

  async create(request: StudentRequest): Promise<ApiResponse<StudentDetail>> {
    if (emailTaken(students, request.email)) return fail('conflict', 'E-mail já cadastrado')
    if (cpfTaken(request.cpf)) return fail('conflict', 'CPF já cadastrado')

    const student: StudentDetail = { id: newId('s'), ...normalize(request) }
    students.push(student)
    return respond(structuredClone(student), 'Aluno cadastrado com sucesso')
  },

  async update(id: string, request: StudentRequest): Promise<ApiResponse<StudentDetail>> {
    const student = students.find((item) => item.id === id)

    if (!student) return fail('not_found', 'Aluno não encontrado')
    if (emailTaken(students, request.email, id)) return fail('conflict', 'E-mail já cadastrado')
    if (cpfTaken(request.cpf, id)) return fail('conflict', 'CPF já cadastrado')

    Object.assign(student, normalize(request))
    return respond(structuredClone(student), 'Aluno atualizado com sucesso')
  },

  async remove(id: string): Promise<ApiResponse<null>> {
    const index = students.findIndex((item) => item.id === id)

    if (index === -1) return fail('not_found', 'Aluno não encontrado')

    if (enrollments.some((enrollment) => enrollment.studentId === id)) {
      return fail('conflict', 'Não é possível excluir um aluno com matrículas')
    }

    students.splice(index, 1)
    return respond(null, 'Aluno excluído com sucesso')
  },
}
