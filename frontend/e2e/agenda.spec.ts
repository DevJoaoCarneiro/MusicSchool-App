import type { Page } from '@playwright/test'
import { dialog, expect, expectToast, heading, selectOption, test } from './support/fixtures'
import { ApiClient } from './support/api'
import { addDays, brDate, brDigits, englishWeekday, uid, weeksFromNow } from './support/data'
import { readState } from './support/state'

const lessonDay = addDays(new Date(), 2)

async function goToWeekOf(page: Page, date: Date) {
  await page.getByRole('button', { name: 'Hoje' }).click()
  for (let index = 0; index < weeksFromNow(date); index += 1) {
    await page.getByRole('button', { name: 'Próximo período' }).click()
  }
}

function lessonCard(page: Page, time: string, student: string) {
  return page.getByRole('button', { name: new RegExp(`^${time} – .*${student}`) })
}

test.describe.serial('Agenda', () => {
  let student = ''
  let teacher = ''
  let enrollmentLabel = ''
  const room = `Sala E2E ${uid()}`

  test.beforeAll(async ({ baseURL }) => {
    const state = readState()
    const api = await ApiClient.withToken(baseURL!, state.sessions.admin.token)
    const createdStudent = await api.createStudent()
    const createdTeacher = await api.createTeacher(state.instrument.name)
    await api.createEnrollment(createdStudent.id, state.instrument.id)
    await api.dispose()

    student = createdStudent.name
    teacher = createdTeacher.name
    enrollmentLabel = `${student} — ${state.instrument.name}`
  })

  test.beforeEach(async ({ page, loginAs }) => {
    await loginAs('admin')
    await page.goto('/agenda')
    await expect(heading(page, 'Agenda')).toBeVisible()
  })

  test('agenda uma aula avulsa', async ({ page }) => {
    await page.getByRole('button', { name: 'Agendar aula' }).click()
    const form = dialog(page)
    await selectOption(form.getByLabel('Matrícula'), enrollmentLabel)
    await selectOption(form.getByLabel('Professor'), teacher)
    await form.getByLabel('Data', { exact: true }).fill(brDigits(lessonDay))
    await form.getByLabel('Horário').fill('1000')
    await expect(form.getByLabel('Horário')).toHaveValue('10:00')
    await form.getByLabel('Duração (min)').fill('60')
    await form.getByLabel('Sala').fill(room)
    await form.getByRole('button', { name: 'Agendar' }).click()
    await expectToast(page, 'Aula agendada com sucesso')

    await goToWeekOf(page, lessonDay)
    await expect(lessonCard(page, '10:00', student)).toContainText('Agendada')
  })

  test('navegação entre semana e mês, períodos e filtros', async ({ page }) => {
    const week = page.getByRole('button', { name: 'Semana' })
    const month = page.getByRole('button', { name: 'Mês' })
    await expect(week).toHaveAttribute('aria-pressed', 'true')

    const label = page.locator('[aria-live="polite"]').first()
    const initialLabel = await label.innerText()
    await page.getByRole('button', { name: 'Próximo período' }).click()
    await expect(label).not.toHaveText(initialLabel)
    await page.getByRole('button', { name: 'Período anterior' }).click()
    await expect(label).toHaveText(initialLabel)

    await month.click()
    await expect(month).toHaveAttribute('aria-pressed', 'true')
    await expect(label).toHaveText(/de \d{4}$/)
    await page.getByRole('button', { name: 'Próximo período' }).click()
    await page.getByRole('button', { name: 'Hoje' }).click()

    const dayButton = page.getByRole('button', { name: `Ver semana de ${brDate(lessonDay)}` })
    if (await dayButton.count() === 0) await page.getByRole('button', { name: 'Próximo período' }).click()
    await page.getByRole('button', { name: `Ver semana de ${brDate(lessonDay)}` }).click()
    await expect(week).toHaveAttribute('aria-pressed', 'true')
    await expect(lessonCard(page, '10:00', student)).toBeVisible()

    await selectOption(page.getByLabel('Professor', { exact: true }), teacher)
    await expect(lessonCard(page, '10:00', student)).toBeVisible()
    await selectOption(page.getByLabel('Aluno', { exact: true }), student)
    await expect(lessonCard(page, '10:00', student)).toBeVisible()
    await page.getByLabel('Professor', { exact: true }).selectOption({ label: 'Todos' })
    await page.getByLabel('Aluno', { exact: true }).selectOption({ label: 'Todos' })
  })

  test('aulas recorrentes mostram as datas em conflito e depois agendam a série', async ({ page }) => {
    await page.getByRole('button', { name: 'Aulas recorrentes' }).click()
    const form = dialog(page)
    await selectOption(form.getByLabel('Matrícula'), enrollmentLabel)
    await selectOption(form.getByLabel('Professor'), teacher)
    await form.getByLabel('Dia da semana').selectOption(englishWeekday(lessonDay))
    await form.getByLabel('Horário').fill('1000')
    await form.getByLabel('Duração (min)').fill('60')
    await form.getByLabel('Sala').fill(room)
    await form.getByLabel('Início').fill(brDigits(lessonDay))
    await form.getByLabel('Fim').fill(brDigits(addDays(lessonDay, 14)))
    await form.getByRole('button', { name: 'Agendar série' }).click()

    await expectToast(page, 'Há conflitos de horário nas aulas recorrentes')
    await expect(form.getByRole('alert')).toContainText('Datas em conflito')
    await expect(form.getByRole('alert')).toContainText(brDate(lessonDay))

    await form.getByLabel('Horário').fill('1500')
    await form.getByRole('button', { name: 'Agendar série' }).click()
    await expectToast(page, 'Aulas recorrentes agendadas com sucesso')

    for (const offset of [0, 7, 14]) {
      await goToWeekOf(page, addDays(lessonDay, offset))
      await expect(lessonCard(page, '15:00', student)).toContainText('Agendada')
    }
  })

  test('detalhe da aula: concluir recusado, reagendar e cancelar', async ({ page }) => {
    await goToWeekOf(page, lessonDay)
    await lessonCard(page, '10:00', student).click()

    const detail = dialog(page)
    await expect(detail).toHaveAccessibleName('Detalhes da aula')
    await expect(detail).toContainText(teacher)
    await expect(detail).toContainText('Aula avulsa')

    await detail.getByRole('button', { name: 'Concluir' }).click()
    await expectToast(page, 'Uma aula que ainda não começou não pode ser concluída')

    await detail.getByRole('button', { name: 'Reagendar' }).click()
    await expect(dialog(page)).toHaveAccessibleName('Reagendar aula')
    await expect(dialog(page).getByLabel('Nova data')).toHaveValue(brDate(lessonDay))
    await dialog(page).getByLabel('Novo horário').fill('1100')
    await dialog(page).getByRole('button', { name: 'Reagendar' }).click()
    await expectToast(page, 'Motivo é obrigatório')
    await dialog(page).getByLabel('Motivo').fill('Pedido do aluno')
    await dialog(page).getByRole('button', { name: 'Reagendar' }).click()
    await expectToast(page, 'Aula reagendada com sucesso')
    await expect(lessonCard(page, '11:00', student)).toBeVisible()

    await lessonCard(page, '11:00', student).click()
    await dialog(page).getByRole('button', { name: 'Cancelar', exact: true }).click()
    await expect(dialog(page)).toHaveAccessibleName('Cancelar aula')
    await dialog(page).getByLabel('Motivo').fill('abc')
    await dialog(page).getByRole('button', { name: 'Cancelar aula' }).click()
    await expectToast(page, /pelo menos 5 caracteres/)
    await dialog(page).getByLabel('Motivo').fill('Aluno doente')
    await dialog(page).getByRole('button', { name: 'Cancelar aula' }).click()
    await expectToast(page, 'Aula cancelada com sucesso')

    await expect(lessonCard(page, '11:00', student)).toContainText('Cancelada')
    await lessonCard(page, '11:00', student).click()
    await expect(dialog(page)).toContainText('Aluno doente')
    await expect(dialog(page).getByRole('button', { name: 'Concluir' })).toHaveCount(0)
    await dialog(page).getByRole('button', { name: 'Fechar' }).last().click()
  })

  test('cancelar série cancela todas as aulas agendadas da série', async ({ page }) => {
    await goToWeekOf(page, lessonDay)
    await lessonCard(page, '15:00', student).click()
    await expect(dialog(page)).toContainText('Faz parte de uma série')

    await dialog(page).getByRole('button', { name: 'Cancelar série' }).click()
    await expect(dialog(page)).toHaveAccessibleName('Cancelar série')
    await dialog(page).getByRole('button', { name: 'Voltar' }).click()
    await expect(dialog(page)).toHaveCount(0)

    await lessonCard(page, '15:00', student).click()
    await dialog(page).getByRole('button', { name: 'Cancelar série' }).click()
    await dialog(page).getByLabel('Motivo').fill('Mudança de horário')
    await dialog(page).getByRole('button', { name: 'Cancelar série' }).click()
    await expectToast(page, 'Série de aulas cancelada com sucesso')

    for (const offset of [0, 7, 14]) {
      await goToWeekOf(page, addDays(lessonDay, offset))
      await expect(lessonCard(page, '15:00', student)).toContainText('Cancelada')
    }
  })
})
