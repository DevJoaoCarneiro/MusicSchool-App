import { expect, test as base, type Locator, type Page } from '@playwright/test'
import { ApiClient, type Session } from './api'
import { readState, type E2EState, type Role } from './state'

const sessionKey = 'musicschool.session'

interface Fixtures {
  state: E2EState
  api: ApiClient
  loginAs: (role: Role, override?: Partial<Session>) => Promise<void>
}

export const test = base.extend<Fixtures>({
  state: async ({}, use) => {
    await use(readState())
  },

  api: async ({ state, baseURL }, use) => {
    const client = await ApiClient.withToken(baseURL!, state.sessions.admin.token)
    await use(client)
    await client.dispose()
  },

  loginAs: async ({ page, state }, use) => {
    await use(async (role, override) => {
      const session = { ...state.sessions[role], ...override }
      await page.addInitScript(
        ([key, value]) => {
          if (!window.sessionStorage.getItem(key)) window.sessionStorage.setItem(key, value)
        },
        [sessionKey, JSON.stringify(session)] as const,
      )
    })
  },
})

export { expect }

export function dialog(page: Page) {
  return page.getByRole('dialog')
}

export async function expectToast(page: Page, text: string | RegExp) {
  await expect(page.getByRole('status')).toContainText(text)
  const closers = page.getByRole('button', { name: 'Fechar aviso' })
  for (const closer of await closers.all()) await closer.click()
  await expect(closers).toHaveCount(0)
}

export async function selectOption(select: Locator, label: string) {
  await expect(select.locator('option', { hasText: label })).not.toHaveCount(0)
  await select.selectOption({ label })
}

export function row(page: Page, text: string) {
  return page.getByRole('row').filter({ hasText: text })
}

export function heading(page: Page, name: string) {
  return page.getByRole('heading', { level: 1, name })
}
