import { parseLocal } from './dates'
import { maskCep, maskCpf, maskPhone } from './masks'

const dateFormatter = new Intl.DateTimeFormat('pt-BR')
const timeFormatter = new Intl.DateTimeFormat('pt-BR', { hour: '2-digit', minute: '2-digit' })
const currencyFormatter = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const monthFormatter = new Intl.DateTimeFormat('pt-BR', { month: 'long', year: 'numeric' })
const weekdayFormatter = new Intl.DateTimeFormat('pt-BR', { weekday: 'short' })

export function formatDate(value: string | Date) {
  return dateFormatter.format(typeof value === 'string' ? parseLocal(value) : value)
}

export function formatTime(value: string | Date) {
  return timeFormatter.format(typeof value === 'string' ? parseLocal(value) : value)
}

export function formatDateTime(value: string) {
  return `${formatDate(value)} às ${formatTime(value)}`
}

export function formatCurrency(value: number) {
  return currencyFormatter.format(value)
}

export function formatReferenceMonth(value: string) {
  const [year, month] = value.split('-')
  return `${month}/${year}`
}

export function formatMonthTitle(date: Date) {
  const text = monthFormatter.format(date)
  return text.charAt(0).toUpperCase() + text.slice(1)
}

export function formatWeekday(date: Date) {
  return weekdayFormatter.format(date).replace('.', '')
}

export function formatPhone(value: string) {
  return maskPhone(value)
}

export function formatCpf(value: string) {
  return maskCpf(value)
}

export function formatCep(value: string) {
  return maskCep(value)
}

export function initials(name: string) {
  const parts = name.trim().split(/\s+/)
  const first = parts[0]?.[0] ?? ''
  const last = parts.length > 1 ? parts[parts.length - 1][0] : ''
  return `${first}${last}`.toUpperCase()
}

export function firstName(name: string) {
  return name.trim().split(/\s+/)[0] ?? name
}
