import { expect, test } from './support/fixtures'

const screens = ['/', '/alunos', '/professores', '/instrumentos', '/matriculas', '/agenda', '/mensalidades', '/usuarios', '/trocar-senha']
const widths = [360, 768, 1024]

async function hiddenWidth(page: import('@playwright/test').Page) {
  return page.evaluate(() => {
    const scroller = document.querySelector('table')?.parentElement
    return {
      page: document.documentElement.scrollWidth - document.documentElement.clientWidth,
      table: scroller ? scroller.scrollWidth - scroller.clientWidth : 0,
    }
  })
}

for (const width of widths) {
  test.describe(`Responsividade em ${width}px`, () => {
    test.use({ viewport: { width, height: 800 } })

    test('nenhuma tela transborda na horizontal', async ({ page, loginAs }) => {
      await loginAs('admin')

      for (const screen of screens) {
        await page.goto(screen)
        await page.waitForLoadState('networkidle')
        expect(await hiddenWidth(page), `${screen} em ${width}px`).toEqual({ page: 0, table: 0 })
      }

      await page.goto('/agenda')
      await page.getByRole('button', { name: 'Mês' }).click()
      expect(await hiddenWidth(page), `agenda (mês) em ${width}px`).toEqual({ page: 0, table: 0 })
    })

    test('tela de login cabe na largura', async ({ page }) => {
      await page.goto('/login')
      expect(await hiddenWidth(page)).toEqual({ page: 0, table: 0 })
    })
  })
}
