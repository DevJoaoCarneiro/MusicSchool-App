import { expect, heading, test } from './support/fixtures'

test.describe('Permissões por perfil', () => {
  test('Secretaria não vê Usuários e não edita instrumentos', async ({ page, loginAs }) => {
    await loginAs('secretary')
    await page.goto('/')

    const menu = page.getByRole('complementary', { name: 'Menu principal' })
    await expect(menu.getByRole('link', { name: 'Usuários' })).toHaveCount(0)
    await expect(menu.getByRole('link', { name: 'Mensalidades' })).toBeVisible()

    await page.goto('/usuarios')
    await expect(page).toHaveURL(/\/$/)

    await page.goto('/instrumentos')
    await expect(heading(page, 'Instrumentos')).toBeVisible()
    await expect(page.getByRole('button', { name: 'Novo instrumento' })).toHaveCount(0)
    await expect(page.getByRole('button', { name: /^Editar / })).toHaveCount(0)

    await page.goto('/alunos')
    await expect(page.getByRole('button', { name: 'Novo aluno' })).toBeVisible()
  })

  test('Professor vê só consulta, sem Mensalidades nem Usuários', async ({ page, loginAs }) => {
    await loginAs('teacher')
    await page.goto('/')

    const menu = page.getByRole('complementary', { name: 'Menu principal' })
    await expect(menu.getByRole('link', { name: 'Mensalidades' })).toHaveCount(0)
    await expect(menu.getByRole('link', { name: 'Usuários' })).toHaveCount(0)

    await page.goto('/mensalidades')
    await expect(page).toHaveURL(/\/$/)

    for (const [path, button] of [
      ['/alunos', 'Novo aluno'],
      ['/professores', 'Novo professor'],
      ['/matriculas', 'Nova matrícula'],
      ['/instrumentos', 'Novo instrumento'],
    ]) {
      await page.goto(path)
      await expect(page.getByRole('heading', { level: 1 })).toBeVisible()
      await expect(page.getByRole('button', { name: button })).toHaveCount(0)
      await expect(page.getByRole('button', { name: /^(Editar|Excluir|Encerrar) / })).toHaveCount(0)
    }

    await page.goto('/agenda')
    await expect(heading(page, 'Minha agenda')).toBeVisible()
    await expect(page.getByRole('button', { name: 'Agendar aula' })).toHaveCount(0)
    await expect(page.getByRole('button', { name: 'Aulas recorrentes' })).toHaveCount(0)
    await expect(page.getByLabel('Professor', { exact: true })).toHaveCount(0)
  })
})
