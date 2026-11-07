import { expect, heading, test } from './support/fixtures'

const adminMenu = [
  { label: 'Alunos', path: '/alunos', title: 'Alunos' },
  { label: 'Professores', path: '/professores', title: 'Professores' },
  { label: 'Instrumentos', path: '/instrumentos', title: 'Instrumentos' },
  { label: 'Matrículas', path: '/matriculas', title: 'Matrículas' },
  { label: 'Agenda', path: '/agenda', title: 'Agenda' },
  { label: 'Mensalidades', path: '/mensalidades', title: 'Mensalidades' },
  { label: 'Usuários', path: '/usuarios', title: 'Usuários' },
  { label: 'Trocar senha', path: '/trocar-senha', title: 'Trocar senha' },
  { label: 'Início', path: '/', title: '' },
]

test.describe('Navegação e layout', () => {
  test('cada item do menu abre a tela certa e fica marcado como ativo', async ({ page, loginAs }) => {
    await loginAs('admin')
    await page.goto('/')

    const menu = page.getByRole('complementary', { name: 'Menu principal' })

    for (const item of adminMenu) {
      const link = menu.getByRole('link', { name: item.label, exact: true })
      await link.click()
      await expect(page).toHaveURL(new RegExp(`${item.path}$`))
      await expect(link).toHaveAttribute('aria-current', 'page')
      if (item.title) await expect(heading(page, item.title)).toBeVisible()
    }
  })

  test('os cards da Início levam às telas', async ({ page, loginAs }) => {
    await loginAs('admin')

    for (const card of ['Alunos', 'Professores', 'Instrumentos', 'Matrículas', 'Agenda', 'Mensalidades']) {
      await page.goto('/')
      await page.getByRole('main').getByRole('link', { name: new RegExp(`^${card}`) }).click()
      await expect(heading(page, card)).toBeVisible()
    }
  })

  test('o topo mostra nome e perfil do usuário', async ({ page, loginAs, state }) => {
    await loginAs('admin')
    await page.goto('/')

    const banner = page.getByRole('banner').first()
    await expect(banner).toContainText(state.sessions.admin.user.name)
    await expect(banner).toContainText('Administrador')
  })

  test('o botão Alternar menu recolhe e expande o menu lateral', async ({ page, loginAs }) => {
    await loginAs('admin')
    await page.goto('/')

    const sidebar = page.getByRole('complementary', { name: 'Menu principal' })
    const toggle = page.getByRole('button', { name: 'Alternar menu' })

    await expect(sidebar).not.toHaveClass(/collapsed/)
    await toggle.click()
    await expect(sidebar).toHaveClass(/collapsed/)
    await expect(sidebar.getByText('Alunos', { exact: true })).toBeHidden()
    await toggle.click()
    await expect(sidebar).not.toHaveClass(/collapsed/)
  })

  test('no celular o menu abre como gaveta e fecha ao navegar', async ({ page, loginAs }) => {
    await page.setViewportSize({ width: 390, height: 844 })
    await loginAs('admin')
    await page.goto('/')

    const sidebar = page.getByRole('complementary', { name: 'Menu principal' })
    await expect(sidebar).toHaveClass(/collapsed/)

    await page.getByRole('button', { name: 'Alternar menu' }).click()
    await expect(sidebar).not.toHaveClass(/collapsed/)

    await sidebar.getByRole('link', { name: 'Professores', exact: true }).click()
    await expect(heading(page, 'Professores')).toBeVisible()
    await expect(sidebar).toHaveClass(/collapsed/)
  })
})
