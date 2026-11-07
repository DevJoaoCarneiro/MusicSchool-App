const runId = `${Date.now().toString(36)}${Math.random().toString(36).slice(2, 5)}`
let sequence = 0

export function uid() {
  sequence += 1
  return `${runId}${sequence}`
}

export function uniqueName(prefix: string) {
  return `${prefix} ${uid()}`
}

export function uniqueEmail(prefix: string) {
  return `${prefix}.${uid()}@e2e.test`
}

function digit(base: number[]) {
  const sum = base.reduce((total, value, index) => total + value * (base.length + 1 - index), 0)
  const rest = sum % 11
  return rest < 2 ? 0 : 11 - rest
}

export function validCpf() {
  let base: number[]
  do {
    base = Array.from({ length: 9 }, () => Math.floor(Math.random() * 10))
  } while (base.every((value) => value === base[0]))

  const first = digit(base)
  const second = digit([...base, first])
  return [...base, first, second].join('')
}

export function mobilePhone() {
  return `119${String(Math.floor(Math.random() * 1e8)).padStart(8, '0')}`
}

const pad = (value: number) => String(value).padStart(2, '0')

export function addDays(date: Date, days: number) {
  const result = new Date(date)
  result.setDate(result.getDate() + days)
  return result
}

export function isoDate(date: Date) {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

export function brDate(date: Date) {
  return `${pad(date.getDate())}/${pad(date.getMonth() + 1)}/${date.getFullYear()}`
}

export function brDigits(date: Date) {
  return brDate(date).replaceAll('/', '')
}

export function brMonth(date: Date) {
  return `${pad(date.getMonth() + 1)}/${date.getFullYear()}`
}

export function englishWeekday(date: Date) {
  return ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'][date.getDay()]
}

function startOfWeek(date: Date) {
  const day = new Date(date.getFullYear(), date.getMonth(), date.getDate())
  day.setDate(day.getDate() - ((day.getDay() + 6) % 7))
  return day
}

export function weeksFromNow(date: Date) {
  const diff = startOfWeek(date).getTime() - startOfWeek(new Date()).getTime()
  return Math.round(diff / (7 * 86_400_000))
}
