import { dialog, expect, expectToast, heading, row, test } from './support/fixtures'
import type { Locator } from '@playwright/test'
import { mobilePhone, uniqueEmail, uniqueName, validCpf } from './support/data'

async function fillPersonal(form: Locator, name: string, birthDate: string, cpf = validCpf()) {
  const personal = form.getByRole('group', { name: 'Dados pessoais' })
  await personal.getByLabel('Nome').fill(name)
  await personal.getByLabel('E-mail').fill(uniqueEmail('aluno'))
  await personal.getByLabel('Telefone').fill(mobilePhone())
  await personal.getByLabel('Nascimento').fill(birthDate)
  await personal.getByLabel('CPF').fill(cpf)

  const address = form.getByRole('group', { name: 'Endereço' })
  await address.getByLabel('Rua').fill('Rua das Flores')
  await address.getByLabel('Número').fill('120')
  await address.getByLabel('Cidade').fill('São Paulo')
  await address.getByLabel('UF').fill('sp')
  await address.getByLabel('CEP').fill('01310100')
}

test.describe('Alunos', () => {
  test.beforeEach(async ({ page, loginAs }) => {
    await loginAs('admin')
    await page.goto('/alunos')
    await expect(heading(page, 'Alunos')).toBeVisible()
  })

  test('aluno menor exige responsável e depois é cadastrado', async ({ page }) => {
    const name = uniqueName('Aluna Menor')

    await page.getByRole('button', { name: 'Novo aluno' }).click()
    const form = dialog(page)
    await fillPersonal(form, name, '14032015')
    await expect(form.getByRole('group', { name: 'Dados pessoais' }).getByLabel('Nascimento')).toHaveValue('14/03/2015')
    await expect(form.getByRole('group', { name: 'Endereço' }).getByLabel('UF')).toHaveValue('SP')

    await form.getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Responsável é obrigatório para alunos menores de 18 anos')

    const guardian = form.getByRole('group', { name: /Responsável/ })
    await guardian.getByLabel('Nome').fill('Responsável Teste')
    await guardian.getByLabel('Telefone').fill(mobilePhone())
    await guardian.getByLabel('E-mail').fill(uniqueEmail('responsavel'))
    await guardian.getByLabel('CPF').fill(validCpf())
    await form.getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Aluno cadastrado com sucesso')

    await page.getByLabel('Buscar alunos por nome').fill(name)
    await expect(row(page, name)).toContainText('14/03/2015')

    await page.getByRole('button', { name: `Ver ${name}` }).click()
    await expect(dialog(page)).toHaveAccessibleName('Detalhes do aluno')
    await expect(dialog(page)).toContainText('Responsável Teste')
    await expect(dialog(page)).toContainText('São Paulo / SP')
  })

  test('recusa CPF inválido', async ({ page }) => {
    await page.getByRole('button', { name: 'Novo aluno' }).click()
    await fillPersonal(dialog(page), uniqueName('Aluno'), '02111998', '11111111111')
    await dialog(page).getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'CPF inválido')
  })

  test('edita pelo lápis e pelo detalhe e exclui aluno sem matrícula', async ({ page, api }) => {
    const student = await api.createStudent()
    await page.getByLabel('Buscar alunos por nome').fill(student.name)

    await page.getByRole('button', { name: `Editar ${student.name}` }).click()
    await expect(dialog(page)).toHaveAccessibleName('Editar aluno')
    await expect(dialog(page).getByRole('group', { name: 'Endereço' }).getByLabel('Rua')).toHaveValue('Rua dos Testes')
    await dialog(page).getByRole('group', { name: 'Endereço' }).getByLabel('Número').fill('999')
    await dialog(page).getByRole('button', { name: 'Salvar' }).click()
    await expectToast(page, 'Aluno atualizado com sucesso')

    await page.getByRole('button', { name: `Ver ${student.name}` }).click()
    await expect(dialog(page)).toContainText('Rua dos Testes, 999')
    await expect(dialog(page)).toContainText('Nenhum responsável cadastrado.')
    await dialog(page).getByRole('button', { name: 'Editar' }).click()
    await expect(dialog(page)).toHaveAccessibleName('Editar aluno')
    await dialog(page).getByRole('button', { name: 'Cancelar' }).click()

    await page.getByRole('button', { name: `Excluir ${student.name}` }).click()
    await dialog(page).getByRole('button', { name: 'Excluir' }).click()
    await expectToast(page, 'Aluno excluído com sucesso')
    await expect(page.getByText('Nenhum aluno encontrado')).toBeVisible()
  })

  test('não exclui aluno com matrícula', async ({ page, api, state }) => {
    const student = await api.createStudent()
    await api.createEnrollment(student.id, state.instrument.id)
    await page.getByLabel('Buscar alunos por nome').fill(student.name)

    await page.getByRole('button', { name: `Excluir ${student.name}` }).click()
    await dialog(page).getByRole('button', { name: 'Excluir' }).click()
    await expectToast(page, 'Não é possível excluir o aluno porque ele tem matrículas ativas')
    await expect(row(page, student.name)).toHaveCount(1)
  })
})
