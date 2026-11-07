import { dialog, expect, expectToast, heading, row, test } from './support/fixtures'
import { mobilePhone, uniqueEmail, uniqueName, validCpf } from './support/data'

test.describe('Professores', () => {
  test.beforeEach(async ({ page, loginAs }) => {
    await loginAs('admin')
    await page.goto('/professores')
    await expect(heading(page, 'Professores')).toBeVisible()
  })

  test('cadastro valida especialidade, busca, detalhe, edição e exclusão', async ({ page, state }) => {
    const name = uniqueName('Professora')
    const cpf = validCpf()

    await page.getByRole('button', { name: 'Novo professor' }).click()
    const form = dialog(page)
    await form.getByLabel('Nome').fill(name)
    await form.getByLabel('E-mail').fill(uniqueEmail('prof'))
    await form.getByLabel('Telefone').fill(mobilePhone())
    await form.getByLabel('CPF').fill(cpf)
    await expect(form.getByLabel('CPF')).toHaveValue(/^\d{3}\.\d{3}\.\d{3}-\d{2}$/)

    await form.getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Informe ao menos uma especialidade')

    const chip = form.getByRole('button', { name: state.instrument.name })
    await chip.click()
    await expect(chip).toHaveAttribute('aria-pressed', 'true')
    await form.getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Professor cadastrado com sucesso')

    await page.getByLabel('Buscar professores por nome').fill(name)
    await expect(page.getByRole('row')).toHaveCount(2)
    await expect(row(page, name)).toContainText(state.instrument.name)

    await page.getByLabel('Buscar professores por nome').fill(uniqueName('ninguem'))
    await expect(page.getByText('Nenhum professor encontrado')).toBeVisible()
    await page.getByLabel('Buscar professores por nome').fill(name)

    await page.getByRole('button', { name: `Ver ${name}` }).click()
    await expect(dialog(page)).toHaveAccessibleName('Detalhes do professor')
    await expect(dialog(page)).toContainText(`${cpf.slice(0, 3)}.${cpf.slice(3, 6)}.${cpf.slice(6, 9)}-${cpf.slice(9)}`)

    await dialog(page).getByRole('button', { name: 'Editar' }).click()
    await expect(dialog(page)).toHaveAccessibleName('Editar professor')
    await expect(dialog(page).getByLabel('Nome')).toHaveValue(name)
    await dialog(page).getByLabel('Telefone').fill('1133334444')
    await dialog(page).getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Professor atualizado com sucesso')

    await page.getByRole('button', { name: `Ver ${name}` }).click()
    await expect(dialog(page)).toContainText('(11) 3333-4444')
    await dialog(page).getByRole('button', { name: 'Fechar' }).last().click()

    await page.getByRole('button', { name: `Excluir ${name}` }).click()
    await dialog(page).getByRole('button', { name: 'Excluir' }).click()
    await expectToast(page, 'Professor excluído com sucesso')
    await expect(row(page, name)).toHaveCount(0)
  })

  test('recusa CPF inválido e e-mail duplicado', async ({ page, api, state }) => {
    const existing = await api.createTeacher(state.instrument.name)

    await page.getByRole('button', { name: 'Novo professor' }).click()
    const form = dialog(page)
    await form.getByLabel('Nome').fill(uniqueName('Professor'))
    await form.getByLabel('E-mail').fill(uniqueEmail('prof'))
    await form.getByLabel('Telefone').fill(mobilePhone())
    await form.getByLabel('CPF').fill('12345678900')
    await form.getByRole('button', { name: state.instrument.name }).click()
    await form.getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'CPF inválido')

    await form.getByLabel('CPF').fill(validCpf())
    await form.getByLabel('E-mail').fill(existing.email)
    await form.getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'E-mail já está em uso')
  })
})
