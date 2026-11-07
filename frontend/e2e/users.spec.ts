import { dialog, expect, expectToast, heading, row, test } from './support/fixtures'
import { uniqueEmail, uniqueName } from './support/data'

test.describe('Usuários', () => {
  test.beforeEach(async ({ loginAs }) => {
    await loginAs('admin')
  })

  test('paginação avança e volta', async ({ page, api }) => {
    const total = await api.countUsers()
    for (let index = total; index < 11; index += 1) await api.createUser('Secretary')

    await page.goto('/usuarios')
    await expect(heading(page, 'Usuários')).toBeVisible()
    await expect(page.getByText(/^1–10 de \d+$/)).toBeVisible()

    const previous = page.getByRole('button', { name: 'Página anterior' })
    const next = page.getByRole('button', { name: 'Próxima página' })

    await expect(previous).toBeDisabled()
    await next.click()
    await expect(page.getByText(/^Página 2 de \d+$/)).toBeVisible()
    await expect(previous).toBeEnabled()
    await previous.click()
    await expect(page.getByText(/^Página 1 de \d+$/)).toBeVisible()
  })

  test('cria usuário com perfil e recusa e-mail repetido', async ({ page }) => {
    const name = uniqueName('Secretária')
    const email = uniqueEmail('secretaria')

    await page.goto('/usuarios')
    await page.getByRole('button', { name: 'Novo usuário' }).click()

    const form = dialog(page)
    await form.getByLabel('Nome').fill(name)
    await form.getByLabel('E-mail').fill(email)
    await form.getByLabel('Senha').fill('Senha@123')
    await form.getByLabel('Perfil').selectOption({ label: 'Secretaria' })
    await form.getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Usuário criado com sucesso')

    await page.getByRole('button', { name: 'Novo usuário' }).click()
    await form.getByLabel('Nome').fill(name)
    await form.getByLabel('E-mail').fill(email)
    await form.getByLabel('Senha').fill('Senha@123')
    await form.getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'E-mail já está em uso')
    await form.getByRole('button', { name: 'Cancelar' }).click()

    while (await row(page, name).count() === 0 && await page.getByRole('button', { name: 'Próxima página' }).isEnabled()) {
      await page.getByRole('button', { name: 'Próxima página' }).click()
    }
    await expect(row(page, name)).toContainText('Secretaria')
  })
})
