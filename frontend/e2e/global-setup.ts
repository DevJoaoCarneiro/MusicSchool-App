import fs from 'node:fs'
import path from 'node:path'
import type { FullConfig } from '@playwright/test'
import { ApiClient, type Session } from './support/api'
import { uniqueEmail, uniqueName } from './support/data'
import { statePath, type E2EState } from './support/state'

export default async function globalSetup(config: FullConfig) {
  const baseURL = config.projects[0].use.baseURL ?? 'http://localhost:5173'
  const adminEmail = process.env.E2E_ADMIN_EMAIL
  const adminPassword = process.env.E2E_ADMIN_PASSWORD

  if (!adminEmail || !adminPassword) {
    throw new Error('Defina E2E_ADMIN_EMAIL e E2E_ADMIN_PASSWORD com um usuário Admin da API antes de rodar os testes.')
  }

  const admin = await ApiClient.login(baseURL, adminEmail, adminPassword)

  try {
    const instrument = await admin.client.createInstrument(uniqueName('Instrumento Base'))
    const password = 'E2e@12345'
    const teacherEmail = uniqueEmail('professor.usuario')
    const teacherName = uniqueName('Professora Logada')

    await admin.client.createTeacher(instrument.name, { name: teacherName, email: teacherEmail })

    const secretary = await admin.client.createUser('Secretary', { password })
    const teacher = await admin.client.createUser('Teacher', { name: teacherName, email: teacherEmail, password })
    const student = await admin.client.createUser('Student', { password })

    const sessions: Record<string, Session> = { admin: admin.session }

    for (const [role, user] of Object.entries({ secretary, teacher, student })) {
      const login = await ApiClient.login(baseURL, user.email, password)
      sessions[role] = login.session
      await login.client.dispose()
    }

    const state: E2EState = {
      admin: { email: adminEmail, password: adminPassword },
      instrument,
      sessions: sessions as E2EState['sessions'],
    }

    fs.mkdirSync(path.dirname(statePath), { recursive: true })
    fs.writeFileSync(statePath, JSON.stringify(state, null, 2))
  } finally {
    await admin.client.dispose()
  }
}
