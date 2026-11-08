import { ApiClient } from './support/api'
import { expect, expectToast, heading, test } from './support/fixtures'

test('trocar senha recusa a atual errada sem deslogar e depois troca', async ({ page, api, baseURL }) => {
  const user = await api.createUser('Secretary')
  const login = await ApiClient.login(baseURL!, user.email, user.password)
  await login.client.dispose()

  await page.addInitScript((value) => {
    if (!window.sessionStorage.getItem('musicschool.session')) {
      window.sessionStorage.setItem('musicschool.session', value)
    }
  }, JSON.stringify(login.session))

  await page.goto('/trocar-senha')
  await expect(heading(page, 'Trocar senha')).toBeVisible()

  await page.getByLabel('Senha atual').fill('senha-errada')
  await page.getByLabel('Nova senha').fill('NovaSenha@1')
  await page.getByRole('button', { name: 'Salvar nova senha' }).click()
  await expectToast(page, 'Senha atual incorreta')
  await expect(page).toHaveURL(/\/trocar-senha$/)

  await page.getByLabel('Senha atual').fill(user.password)
  await page.getByLabel('Nova senha').fill(user.password)
  await page.getByRole('button', { name: 'Salvar nova senha' }).click()
  await expectToast(page, 'A nova senha deve ser diferente da atual')

  await page.getByLabel('Nova senha').fill('NovaSenha@1')
  await page.getByRole('button', { name: 'Salvar nova senha' }).click()
  await expectToast(page, 'Senha alterada com sucesso')
  await expect(page.getByLabel('Senha atual')).toHaveValue('')

  const relogin = await ApiClient.login(baseURL!, user.email, 'NovaSenha@1')
  await relogin.client.dispose()
})
