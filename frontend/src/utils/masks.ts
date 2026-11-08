export function onlyDigits(value: string) {
  return value.replace(/\D/g, '')
}

function applyPattern(digits: string, pattern: string) {
  let result = ''
  let index = 0

  for (const char of pattern) {
    if (index >= digits.length) break

    if (char === '#') {
      result += digits[index]
      index += 1
    } else {
      result += char
    }
  }

  return result
}

export function maskPhone(value: string) {
  const digits = onlyDigits(value).slice(0, 11)
  return applyPattern(digits, digits.length > 10 ? '(##) #####-####' : '(##) ####-####')
}

export function maskCpf(value: string) {
  return applyPattern(onlyDigits(value).slice(0, 11), '###.###.###-##')
}

export function maskCep(value: string) {
  return applyPattern(onlyDigits(value).slice(0, 8), '#####-###')
}

export function maskDate(value: string) {
  return applyPattern(onlyDigits(value).slice(0, 8), '##/##/####')
}

export function maskMonth(value: string) {
  return applyPattern(onlyDigits(value).slice(0, 6), '##/####')
}

export function maskTime(value: string) {
  return applyPattern(onlyDigits(value).slice(0, 4), '##:##')
}

export function maskUf(value: string) {
  return value.replace(/[^a-zA-Z]/g, '').slice(0, 2).toUpperCase()
}

export function maskCurrency(value: string) {
  const digits = onlyDigits(value).slice(0, 12)
  if (!digits) return ''

  const cents = Number(digits) / 100
  return cents.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

export function parseCurrency(value: string) {
  const digits = onlyDigits(value)
  return digits ? Number(digits) / 100 : 0
}

export function currencyToMask(value: number) {
  return value.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

export function brDateToIso(value: string) {
  const [day, month, year] = value.split('/')
  if (!day || !month || year?.length !== 4) return ''
  return `${year}-${month}-${day}`
}

export function isoToBrDate(value: string) {
  const [year, month, day] = value.slice(0, 10).split('-')
  if (!year || !month || !day) return ''
  return `${day}/${month}/${year}`
}

export function brMonthToIso(value: string) {
  const [month, year] = value.split('/')
  if (!month || year?.length !== 4) return ''
  return `${year}-${month}`
}

export function isoToBrMonth(value: string) {
  const [year, month] = value.split('-')
  if (!year || !month) return ''
  return `${month}/${year}`
}
