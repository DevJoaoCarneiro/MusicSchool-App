import { dialog, expect, expectToast, heading, row, test } from './support/fixtures'
import { uniqueName } from './support/data'

test.describe('Instrumentos', () => {
  test.beforeEach(async ({ page, loginAs }) => {
    await loginAs('admin')
    await page.goto('/instrumentos')
    await expect(heading(page, 'Instrumentos')).toBeVisible()
  })

  test('cria, recusa duplicado, edita e exclui', async ({ page }) => {
    const name = uniqueName('Teclado')

    await page.getByRole('button', { name: 'Novo instrumento' }).click()
    await expect(dialog(page)).toHaveAccessibleName('Novo instrumento')
    await dialog(page).getByLabel('Nome').fill(name)
    await dialog(page).getByLabel('Descrição').fill('Criado pelo teste')
    await dialog(page).getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Instrumento criado com sucesso')
    await expect(row(page, name)).toContainText('Criado pelo teste')

    await page.getByRole('button', { name: 'Novo instrumento' }).click()
    await dialog(page).getByLabel('Nome').fill(name.toUpperCase())
    await dialog(page).getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Já existe um instrumento com este nome')
    await dialog(page).getByRole('button', { name: 'Cancelar' }).click()
    await expect(dialog(page)).toHaveCount(0)

    await page.getByRole('button', { name: `Editar ${name}` }).click()
    await expect(dialog(page)).toHaveAccessibleName('Editar instrumento')
    await expect(dialog(page).getByLabel('Nome')).toHaveValue(name)
    await dialog(page).getByLabel('Descrição').fill('Descrição editada')
    await dialog(page).getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Instrumento atualizado com sucesso')
    await expect(row(page, name)).toContainText('Descrição editada')

    await page.getByRole('button', { name: `Excluir ${name}` }).click()
    await expect(dialog(page)).toHaveAccessibleName('Excluir instrumento')
    await dialog(page).getByRole('button', { name: 'Cancelar' }).click()
    await expect(row(page, name)).toHaveCount(1)

    await page.getByRole('button', { name: `Excluir ${name}` }).click()
    await dialog(page).getByRole('button', { name: 'Excluir' }).click()
    await expectToast(page, 'Instrumento excluído com sucesso')
    await expect(row(page, name)).toHaveCount(0)
  })

  test('não exclui instrumento com matrícula', async ({ page, api }) => {
    const instrument = await api.createInstrument()
    const student = await api.createStudent()
    await api.createEnrollment(student.id, instrument.id)
    await page.reload()

    await page.getByRole('button', { name: `Excluir ${instrument.name}` }).click()
    await dialog(page).getByRole('button', { name: 'Excluir' }).click()
    await expectToast(page, 'O instrumento possui matrículas ativas')
    await expect(row(page, instrument.name)).toHaveCount(1)
  })

  test('modal fecha pelo X e pela tecla Esc', async ({ page }) => {
    await page.getByRole('button', { name: 'Novo instrumento' }).click()
    await dialog(page).getByRole('button', { name: 'Fechar' }).click()
    await expect(dialog(page)).toHaveCount(0)

    await page.getByRole('button', { name: 'Novo instrumento' }).click()
    await page.keyboard.press('Escape')
    await expect(dialog(page)).toHaveCount(0)
  })
})
