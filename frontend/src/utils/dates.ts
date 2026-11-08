function pad(value: number) {
  return String(value).padStart(2, '0')
}

export function toIsoDate(date: Date) {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

export function toLocalIso(date: Date) {
  return `${toIsoDate(date)}T${pad(date.getHours())}:${pad(date.getMinutes())}:00`
}

export function toIsoMonth(date: Date) {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}`
}

export function parseLocal(value: string) {
  return new Date(value.length <= 10 ? `${value}T00:00:00` : value)
}

export function addDays(date: Date, days: number) {
  const result = new Date(date)
  result.setDate(result.getDate() + days)
  return result
}

export function addMinutes(date: Date, minutes: number) {
  return new Date(date.getTime() + minutes * 60_000)
}

export function startOfDay(date: Date) {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate())
}

export function startOfWeek(date: Date) {
  const day = startOfDay(date)
  const offset = (day.getDay() + 6) % 7
  return addDays(day, -offset)
}

export function startOfMonth(date: Date) {
  return new Date(date.getFullYear(), date.getMonth(), 1)
}

export function isSameDay(a: Date, b: Date) {
  return toIsoDate(a) === toIsoDate(b)
}

export function combineDateTime(isoDate: string, time: string) {
  return `${isoDate}T${time}:00`
}
