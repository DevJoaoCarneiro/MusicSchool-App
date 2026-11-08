import fs from 'node:fs'
import path from 'node:path'
import type { Session } from './api'

export type Role = 'admin' | 'secretary' | 'teacher' | 'student'

export interface E2EState {
  admin: { email: string; password: string }
  instrument: { id: string; name: string }
  sessions: Record<Role, Session>
}

export const statePath = path.join(process.cwd(), 'e2e', '.auth', 'state.json')

export function readState(): E2EState {
  return JSON.parse(fs.readFileSync(statePath, 'utf8')) as E2EState
}
