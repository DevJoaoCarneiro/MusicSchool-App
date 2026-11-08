import { dialog, expect, expectToast, heading, row, selectOption, test } from './support/fixtures'
import { brDate, brDigits } from './support/data'

test.describe('Matrículas', () => {
  test.beforeEach(async ({ page, loginAs }) => {
    await loginAs('admin')
    await page.goto('/matriculas')
    await expect(heading(page, 'Matrículas')).toBeVisible()
  })

  test('cria matrícula, recusa duplicada e valida campos', async ({ page, api, state }) => {
    const student = await api.createStudent()
    await page.reload()

    await page.getByRole('button', { name: 'Nova matrícula' }).click()
    const form = dialog(page)
    await form.getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, /obrigatóri|deve estar entre/)

    await selectOption(form.getByLabel('Aluno'), student.name)
    await selectOption(form.getByLabel('Instrumento'), state.instrument.name)
    await form.getByLabel('Data de início').fill(brDigits(new Date()))
    await form.getByLabel('Mensalidade (R$)').fill('27550')
    await expect(form.getByLabel('Mensalidade (R$)')).toHaveValue('275,50')
    await form.getByLabel('Dia de vencimento').fill('10')
    await form.getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Matrícula criada com sucesso')

    await page.getByRole('button', { name: 'Nova matrícula' }).click()
    await selectOption(form.getByLabel('Aluno'), student.name)
    await selectOption(form.getByLabel('Instrumento'), state.instrument.name)
    await form.getByLabel('Data de início').fill(brDigits(new Date()))
    await form.getByLabel('Mensalidade (R$)').fill('27550')
    await form.getByLabel('Dia de vencimento').fill('10')
    await form.getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Aluno já possui matrícula ativa neste instrumento')
    await form.getByRole('button', { name: 'Cancelar' }).click()

    await selectOption(page.getByLabel('Aluno', { exact: true }), student.name)
    await expect(page.getByRole('row')).toHaveCount(2)
    await expect(row(page, student.name)).toContainText('Ativa')
    await expect(row(page, student.name)).toContainText(brDate(new Date()))
  })

  test('filtros, detalhe e encerramento', async ({ page, api, state }) => {
    const student = await api.createStudent()
    await api.createEnrollment(student.id, state.instrument.id)
    await page.reload()

    await selectOption(page.getByLabel('Aluno', { exact: true }), student.name)
    await selectOption(page.getByLabel('Instrumento', { exact: true }), state.instrument.name)
    await page.getByLabel('Status', { exact: true }).selectOption({ label: 'Encerrada' })
    await expect(page.getByText('Nenhuma matrícula encontrada')).toBeVisible()
    await page.getByLabel('Status', { exact: true }).selectOption({ label: 'Ativa' })
    await expect(row(page, student.name)).toHaveCount(1)

    await page.getByRole('button', { name: `Ver matrícula de ${student.name}` }).click()
    await expect(dialog(page)).toHaveAccessibleName('Detalhes da matrícula')
    await expect(dialog(page)).toContainText('R$ 250,00')
    await expect(dialog(page)).toContainText('Todo dia 5')

    await dialog(page).getByRole('button', { name: 'Encerrar matrícula' }).click()
    await expect(dialog(page)).toHaveAccessibleName('Encerrar matrícula')
    await dialog(page).getByRole('button', { name: 'Encerrar' }).click()
    await expectToast(page, 'Data de término é obrigatória')

    await dialog(page).getByLabel('Data de término').fill(brDigits(new Date()))
    await dialog(page).getByRole('button', { name: 'Encerrar' }).click()
    await expectToast(page, 'Motivo é obrigatório')

    await dialog(page).getByLabel('Motivo').fill('Mudou de cidade')
    await dialog(page).getByRole('button', { name: 'Encerrar' }).click()
    await expectToast(page, 'Matrícula encerrada com sucesso')

    await page.getByLabel('Status', { exact: true }).selectOption({ label: 'Encerrada' })
    await expect(row(page, student.name)).toContainText('Encerrada')
    await expect(page.getByRole('button', { name: `Encerrar matrícula de ${student.name}` })).toHaveCount(0)
  })

  test('botão de encerrar na linha abre o modal', async ({ page, api, state }) => {
    const student = await api.createStudent()
    await api.createEnrollment(student.id, state.instrument.id)
    await page.reload()
    await selectOption(page.getByLabel('Aluno', { exact: true }), student.name)

    await page.getByRole('button', { name: `Encerrar matrícula de ${student.name}` }).click()
    await expect(dialog(page)).toHaveAccessibleName('Encerrar matrícula')
    await expect(dialog(page)).toContainText(student.name)
    await dialog(page).getByRole('button', { name: 'Cancelar' }).click()
    await expect(dialog(page)).toHaveCount(0)
  })
})
