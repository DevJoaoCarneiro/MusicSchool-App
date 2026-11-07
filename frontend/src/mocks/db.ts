import type {
  EnrollmentStatus,
  Instrument,
  LessonStatus,
  PaymentMethod,
  StudentDetail,
  TeacherDetail,
  TuitionStatus,
  UserListItem,
  UserRole,
} from '@/types/entities'
import { addDays, startOfWeek, toIsoDate, toIsoMonth, toLocalIso } from '@/utils/dates'

export interface MockAccount extends UserListItem {
  password: string
}

export interface MockEnrollment {
  id: string
  studentId: string
  instrumentId: string
  startDate: string
  monthlyFee: number
  dueDay: number
  status: EnrollmentStatus
  endDate: string | null
  reason: string | null
}

export interface MockLesson {
  id: string
  seriesId: string | null
  enrollmentId: string
  teacherId: string
  startsAt: string
  durationMinutes: number
  room: string
  status: LessonStatus
  reason: string | null
  cancelledAt: string | null
}

export interface MockTuition {
  id: string
  enrollmentId: string
  referenceMonth: string
  amount: number
  dueDate: string
  status: TuitionStatus
  paidAt: string | null
  paidAmount: number | null
  method: PaymentMethod | null
}

const extraRoles: UserRole[] = ['Teacher', 'Secretary', 'Student']

export const accounts: MockAccount[] = [
  { id: 'u-1', name: 'Helena Duarte', email: 'admin@escola.com', role: 'Admin', password: '123456' },
  { id: 'u-2', name: 'Marcos Lima', email: 'secretaria@escola.com', role: 'Secretary', password: '123456' },
  { id: 'u-3', name: 'Rafael Souza', email: 'professor@escola.com', role: 'Teacher', password: '123456' },
  { id: 'u-4', name: 'Lia Campos', email: 'aluno@escola.com', role: 'Student', password: '123456' },
  { id: 'u-5', name: 'Paulo Campos', email: 'responsavel@escola.com', role: 'Guardian', password: '123456' },
  ...Array.from({ length: 23 }, (_, index) => {
    const number = index + 6
    return {
      id: `u-${number}`,
      name: `Usuário ${number}`,
      email: `usuario${number}@escola.com`,
      role: extraRoles[index % extraRoles.length],
      password: '123456',
    }
  }),
]

export const instruments: Instrument[] = [
  { id: 'i-1', name: 'Piano', description: 'Teclado acústico e digital' },
  { id: 'i-2', name: 'Violão', description: 'Cordas de nylon e aço' },
  { id: 'i-3', name: 'Violino', description: 'Cordas friccionadas' },
  { id: 'i-4', name: 'Bateria', description: 'Percussão acústica' },
  { id: 'i-5', name: 'Canto', description: 'Técnica vocal' },
  { id: 'i-6', name: 'Saxofone', description: 'Sopro, palheta simples' },
]

export const teachers: TeacherDetail[] = [
  {
    id: 't-1',
    name: 'Rafael Souza',
    email: 'professor@escola.com',
    phone: '11944443333',
    cpf: '52998224725',
    specialties: ['Piano', 'Canto'],
  },
  {
    id: 't-2',
    name: 'Ana Beatriz Rocha',
    email: 'ana.rocha@escola.com',
    phone: '11922221111',
    cpf: '15350946056',
    specialties: ['Violino'],
  },
  {
    id: 't-3',
    name: 'Carlos Mendes',
    email: 'carlos@escola.com',
    phone: '21977776666',
    cpf: '86288366757',
    specialties: ['Violão', 'Bateria'],
  },
]

export const students: StudentDetail[] = [
  {
    id: 's-1',
    name: 'Lia Campos',
    email: 'aluno@escola.com',
    phone: '11955554444',
    cpf: '39053344705',
    birthDate: '2012-03-14',
    address: { street: 'Rua das Flores', number: '120', city: 'São Paulo', state: 'SP', zipCode: '01310100' },
    guardian: { name: 'Paulo Campos', phone: '11955550000', email: 'responsavel@escola.com', cpf: '11144477735' },
  },
  {
    id: 's-2',
    name: 'Bruno Teixeira',
    email: 'bruno@email.com',
    phone: '11933332222',
    cpf: '46559808052',
    birthDate: '1998-11-02',
    address: { street: 'Av. Paulista', number: '1500', city: 'São Paulo', state: 'SP', zipCode: '01310200' },
    guardian: null,
  },
  {
    id: 's-3',
    name: 'Júlia Ferraz',
    email: 'julia@email.com',
    phone: '21988887777',
    cpf: '74682489070',
    birthDate: '2005-07-21',
    address: { street: 'Rua Voluntários da Pátria', number: '45', city: 'Rio de Janeiro', state: 'RJ', zipCode: '22270000' },
    guardian: null,
  },
]

const today = new Date()
const currentMonth = toIsoMonth(today)
const previousMonth = toIsoMonth(new Date(today.getFullYear(), today.getMonth() - 1, 1))

function monthsAgo(months: number) {
  return toIsoDate(new Date(today.getFullYear(), today.getMonth() - months, 5))
}

export const enrollments: MockEnrollment[] = [
  { id: 'e-1', studentId: 's-1', instrumentId: 'i-1', startDate: monthsAgo(4), monthlyFee: 280, dueDay: 10, status: 'Active', endDate: null, reason: null },
  { id: 'e-2', studentId: 's-2', instrumentId: 'i-2', startDate: monthsAgo(3), monthlyFee: 250, dueDay: 5, status: 'Active', endDate: null, reason: null },
  { id: 'e-3', studentId: 's-3', instrumentId: 'i-3', startDate: monthsAgo(2), monthlyFee: 300, dueDay: 15, status: 'Active', endDate: null, reason: null },
  { id: 'e-4', studentId: 's-1', instrumentId: 'i-5', startDate: monthsAgo(1), monthlyFee: 220, dueDay: 10, status: 'Active', endDate: null, reason: null },
  { id: 'e-5', studentId: 's-2', instrumentId: 'i-4', startDate: monthsAgo(8), monthlyFee: 260, dueDay: 20, status: 'Closed', endDate: monthsAgo(2), reason: 'Mudou de cidade' },
]

function lessonAt(weekOffset: number, dayIndex: number, hour: number, minute = 0) {
  const date = addDays(startOfWeek(today), weekOffset * 7 + dayIndex)
  date.setHours(hour, minute, 0, 0)
  return toLocalIso(date)
}

function buildSeries(
  seriesId: string,
  enrollmentId: string,
  teacherId: string,
  dayIndex: number,
  hour: number,
  room: string,
): MockLesson[] {
  return [-1, 0, 1, 2, 3].map((weekOffset) => ({
    id: `${seriesId}-${weekOffset + 1}`,
    seriesId,
    enrollmentId,
    teacherId,
    startsAt: lessonAt(weekOffset, dayIndex, hour),
    durationMinutes: 60,
    room,
    status: weekOffset < 0 ? 'Completed' : 'Scheduled',
    reason: null,
    cancelledAt: null,
  }))
}

export const lessons: MockLesson[] = [
  ...buildSeries('ls-1', 'e-1', 't-1', 0, 9, 'Sala 1'),
  ...buildSeries('ls-2', 'e-2', 't-3', 1, 14, 'Sala 2'),
  ...buildSeries('ls-3', 'e-3', 't-2', 3, 16, 'Sala 3'),
  {
    id: 'l-1',
    seriesId: null,
    enrollmentId: 'e-4',
    teacherId: 't-1',
    startsAt: lessonAt(0, 2, 10, 30),
    durationMinutes: 45,
    room: 'Sala 1',
    status: 'Scheduled',
    reason: null,
    cancelledAt: null,
  },
  {
    id: 'l-2',
    seriesId: null,
    enrollmentId: 'e-4',
    teacherId: 't-1',
    startsAt: lessonAt(0, 4, 11),
    durationMinutes: 45,
    room: 'Sala 1',
    status: 'Cancelled',
    reason: 'Aluno doente',
    cancelledAt: lessonAt(0, 3, 18),
  },
]

function dueDateFor(referenceMonth: string, dueDay: number) {
  return `${referenceMonth}-${String(dueDay).padStart(2, '0')}`
}

export const tuitions: MockTuition[] = [
  ...['e-1', 'e-2', 'e-3'].map((enrollmentId, index) => {
    const enrollment = enrollments.find((item) => item.id === enrollmentId)!
    return {
      id: `tu-p-${index + 1}`,
      enrollmentId,
      referenceMonth: previousMonth,
      amount: enrollment.monthlyFee,
      dueDate: dueDateFor(previousMonth, enrollment.dueDay),
      status: 'Paid' as const,
      paidAt: dueDateFor(previousMonth, Math.max(1, enrollment.dueDay - 2)),
      paidAmount: enrollment.monthlyFee,
      method: (['Pix', 'Card', 'Cash'] as const)[index],
    }
  }),
  ...['e-1', 'e-2', 'e-3', 'e-4'].map((enrollmentId, index) => {
    const enrollment = enrollments.find((item) => item.id === enrollmentId)!
    return {
      id: `tu-c-${index + 1}`,
      enrollmentId,
      referenceMonth: currentMonth,
      amount: enrollment.monthlyFee,
      dueDate: dueDateFor(currentMonth, enrollment.dueDay),
      status: 'Pending' as const,
      paidAt: null,
      paidAmount: null,
      method: null,
    }
  }),
]

export function dueDateForMonth(referenceMonth: string, dueDay: number) {
  return dueDateFor(referenceMonth, dueDay)
}
