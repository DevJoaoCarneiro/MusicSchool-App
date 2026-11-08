import { dialog, expect, expectToast, heading, row, selectOption, test } from './support/fixtures'
import { brDate, brMonth } from './support/data'

test.describe('Mensalidades', () => {
  test.beforeEach(async ({ loginAs }) => {
    await loginAs('secretary')
  })

  test('gera mensalidades, filtra e registra pagamento', async ({ page, api, state }) => {
    const student = await api.createStudent()
    await api.createEnrollment(student.id, state.instrument.id)

    await page.goto('/mensalidades')
    await expect(heading(page, 'Mensalidades')).toBeVisible()

    await page.getByRole('button', { name: 'Gerar mensalidades' }).click()
    const modal = dialog(page)
    await expect(modal.getByLabel('Mês de referência')).toHaveValue(brMonth(new Date()))
    await modal.getByRole('button', { name: 'Gerar' }).click()
    await expectToast(page, 'Mensalidades geradas com sucesso')
    await expect(modal).toContainText('Criadas')
    await expect(modal).toContainText('Já existentes')
    await modal.getByRole('button', { name: 'Fechar' }).last().click()
    await expect(modal).toHaveCount(0)

    await page.getByLabel('Mês de referência').fill(brMonth(new Date()).replace('/', ''))
    await selectOption(page.getByLabel('Aluno', { exact: true }), student.name)
    await page.getByLabel('Status', { exact: true }).selectOption({ label: 'Pendente' })
    await expect(row(page, student.name)).toContainText('Pendente')
    await expect(row(page, student.name)).toContainText('R$ 250,00')

    await page.getByRole('button', { name: `Registrar pagamento de ${student.name}` }).click()
    await expect(modal).toHaveAccessibleName('Registrar pagamento')
    await expect(modal.getByLabel('Data')).toHaveValue(brDate(new Date()))
    await expect(modal.getByLabel('Valor (R$)')).toHaveValue('250,00')
    await modal.getByRole('button', { name: 'Registrar' }).click()
    await expectToast(page, /Forma de pagamento é obrigatória/)

    await modal.getByLabel('Forma de pagamento').selectOption({ label: 'Pix' })
    await modal.getByRole('button', { name: 'Registrar' }).click()
    await expectToast(page, 'Pagamento registrado com sucesso')

    await expect(page.getByText('Nenhuma mensalidade encontrada')).toBeVisible()
    await page.getByLabel('Status', { exact: true }).selectOption({ label: 'Paga' })
    await expect(row(page, student.name)).toContainText('Paga')
    await expect(row(page, student.name)).toContainText('Pix · R$ 250,00')
    await expect(page.getByRole('button', { name: `Registrar pagamento de ${student.name}` })).toHaveCount(0)
  })

  test('gerar de novo o mesmo mês não duplica', async ({ page }) => {
    await page.goto('/mensalidades')

    await page.getByRole('button', { name: 'Gerar mensalidades' }).click()
    await dialog(page).getByRole('button', { name: 'Gerar' }).click()
    await expectToast(page, 'Mensalidades geradas com sucesso')
    await dialog(page).getByRole('button', { name: 'Fechar' }).last().click()

    await page.getByRole('button', { name: 'Gerar mensalidades' }).click()
    await dialog(page).getByRole('button', { name: 'Gerar' }).click()
    await expectToast(page, 'Mensalidades geradas com sucesso')
    await expect(dialog(page).getByText('Criadas').locator('..')).toContainText('0')
  })

  test('recusa mês de referência inválido', async ({ page }) => {
    await page.goto('/mensalidades')
    await page.getByRole('button', { name: 'Gerar mensalidades' }).click()
    await dialog(page).getByLabel('Mês de referência').fill('132026')
    await dialog(page).getByRole('button', { name: 'Gerar' }).click()
    await expectToast(page, /Mês de referência/)
    await dialog(page).getByRole('button', { name: 'Cancelar' }).click()
    await expect(dialog(page)).toHaveCount(0)
  })
})
