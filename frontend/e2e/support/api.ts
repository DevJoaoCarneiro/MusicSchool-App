import { request, type APIRequestContext } from '@playwright/test'
import { isoDate, mobilePhone, uniqueEmail, uniqueName, validCpf } from './data'

export interface Session {
  token: string
  expiresIn: number
  user: { id: string; name: string; email: string; role: string }
}

interface Envelope<T> {
  message: string
  status: string
  data: T
}

export class ApiClient {
  private constructor(
    private readonly context: APIRequestContext,
    private readonly token: string,
  ) {}

  static async login(baseURL: string, email: string, password: string) {
    const context = await request.newContext({ baseURL })
    const response = await context.post('/api/auth/login', { data: { email, password } })
    const body = (await response.json().catch(() => null)) as Envelope<Session> | null

    if (!body || body.status !== 'Success') {
      await context.dispose()
      throw new Error(`Falha no login de ${email}: ${body?.message ?? response.status()}`)
    }

    return { client: new ApiClient(context, body.data.token), session: body.data }
  }

  static async withToken(baseURL: string, token: string) {
    return new ApiClient(await request.newContext({ baseURL }), token)
  }

  dispose() {
    return this.context.dispose()
  }

  private async send<T>(method: string, path: string, data?: unknown): Promise<T> {
    const response = await this.context.fetch(`/api${path}`, {
      method,
      data,
      headers: { Authorization: `Bearer ${this.token}` },
    })
    const body = (await response.json().catch(() => null)) as Envelope<T> | null

    if (!body || body.status !== 'Success') {
      throw new Error(`${method} ${path} falhou: ${body?.message ?? response.status()}`)
    }

    return body.data
  }

  createInstrument(name = uniqueName('Instrumento E2E')) {
    return this.send<{ id: string; name: string }>('POST', '/instruments', { name, description: 'Criado pelos testes' })
  }

  createTeacher(specialty: string, overrides: { name?: string; email?: string } = {}) {
    return this.send<{ id: string; name: string; email: string }>('POST', '/teachers', {
      name: overrides.name ?? uniqueName('Professor E2E'),
      email: overrides.email ?? uniqueEmail('professor'),
      phone: mobilePhone(),
      cpf: validCpf(),
      specialties: [specialty],
    })
  }

  createStudent(name = uniqueName('Aluno E2E')) {
    return this.send<{ id: string; name: string }>('POST', '/students', {
      name,
      email: uniqueEmail('aluno'),
      phone: mobilePhone(),
      cpf: validCpf(),
      birthDate: '1995-05-10',
      address: { street: 'Rua dos Testes', number: '10', city: 'Sao Paulo', state: 'SP', zipCode: '01310100' },
      guardian: null,
    })
  }

  createEnrollment(studentId: string, instrumentId: string) {
    return this.send<{ id: string }>('POST', '/enrollments', {
      studentId,
      instrumentId,
      startDate: isoDate(new Date()),
      monthlyFee: 250,
      dueDay: 5,
    })
  }

  createUser(role: string, overrides: { name?: string; email?: string; password?: string } = {}) {
    const user = {
      name: overrides.name ?? uniqueName(`Usuario ${role}`),
      email: overrides.email ?? uniqueEmail(role.toLowerCase()),
      password: overrides.password ?? 'E2e@12345',
      role,
    }
    return this.send<{ id: string }>('POST', '/users', user).then(() => user)
  }

  countUsers() {
    return this.send<{ total: number }>('GET', '/users?page=1&pageSize=1').then((data) => data.total)
  }
}
