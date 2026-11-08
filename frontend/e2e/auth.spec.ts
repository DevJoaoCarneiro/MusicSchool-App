import { expect, heading, test } from './support/fixtures'

test.describe('Autenticação', () => {
  test('login com senha errada mostra a mensagem da API e continua na tela', async ({ page, state }) => {
    await page.goto('/login')
    await page.getByLabel('E-mail').fill(state.admin.email)
    await page.getByLabel('Senha').fill('senha-errada')
    await page.getByRole('button', { name: 'Entrar' }).click()

    await expect(page.getByRole('alert')).toHaveText('E-mail ou senha inválidos')
    await expect(page).toHaveURL(/\/login$/)
  })

  test('login com sucesso abre a Início e o botão Sair encerra a sessão', async ({ page, state }) => {
    await page.goto('/login')
    await page.getByLabel('E-mail').fill(state.admin.email)
    await page.getByLabel('Senha').fill(state.admin.password)
    await page.getByRole('button', { name: 'Entrar' }).click()

    await expect(page).toHaveURL(/\/$/)
    await expect(page.getByRole('heading', { level: 1 })).toContainText('Olá,')

    await page.getByRole('button', { name: 'Sair' }).click()
    await expect(page).toHaveURL(/\/login$/)

    await page.goto('/alunos')
    await expect(page).toHaveURL(/\/login$/)
  })

  test('rota protegida sem sessão redireciona para o login', async ({ page }) => {
    await page.goto('/matriculas')
    await expect(page).toHaveURL(/\/login$/)
    await expect(page.getByRole('heading', { name: 'Bem-vindo de volta' })).toBeVisible()
  })

  test('usuário logado que abre /login volta para a Início', async ({ page, loginAs }) => {
    await loginAs('admin')
    await page.goto('/login')
    await expect(page).toHaveURL(/\/$/)
  })

  test('perfil Aluno vai para a tela sem acesso e o botão volta ao login', async ({ page, loginAs }) => {
    await loginAs('student')
    await page.goto('/')

    await expect(page).toHaveURL(/\/sem-acesso$/)
    await expect(page.getByRole('heading', { name: 'Área ainda não disponível' })).toBeVisible()

    await page.getByRole('button', { name: 'Voltar para o login' }).click()
    await expect(page).toHaveURL(/\/login$/)
  })

  test('token inválido encerra a sessão quando a API responde 401', async ({ page, loginAs }) => {
    await loginAs('admin', { token: 'token-invalido' })
    await page.goto('/alunos')
    await expect(page).toHaveURL(/\/login$/)
  })

  test('página inexistente mostra 404 e o botão leva à Início', async ({ page, loginAs }) => {
    await loginAs('admin')
    await page.goto('/rota-que-nao-existe')

    await expect(heading(page, 'Página não encontrada')).toBeVisible()
    await page.getByRole('button', { name: 'Ir para o início' }).click()
    await expect(page).toHaveURL(/\/$/)
  })
})
