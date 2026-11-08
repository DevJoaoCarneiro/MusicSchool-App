export type UserRole = 'Admin' | 'Secretary' | 'Teacher' | 'Student' | 'Guardian'

export interface AuthUser {
  id: string
  name: string
  email: string
  role: UserRole
}

export interface AuthSession {
  token: string
  expiresIn: number
  user: AuthUser
}

export interface LoginRequest {
  email: string
  password: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface UserListItem {
  id: string
  name: string
  email: string
  role: UserRole
}

export interface UserRequest {
  name: string
  email: string
  password: string
  role: UserRole
}

export interface Instrument {
  id: string
  name: string
  description: string
}

export interface InstrumentRequest {
  name: string
  description: string
}

export interface TeacherListItem {
  id: string
  name: string
  email: string
  specialties: string[]
}

export interface TeacherRequest {
  name: string
  email: string
  phone: string
  cpf: string
  specialties: string[]
}

export interface StudentListItem {
  id: string
  name: string
  email: string
  phone: string
  birthDate: string
}

export interface Address {
  street: string
  number: string
  city: string
  state: string
  zipCode: string
}

export interface Guardian {
  name: string
  phone: string
  email: string
  cpf: string
}

export interface StudentRequest {
  name: string
  email: string
  phone: string
  cpf: string
  birthDate: string
  address: Address
  guardian: Guardian | null
}

export interface StudentDetail extends StudentRequest {
  id: string
}

export interface TeacherDetail extends TeacherRequest {
  id: string
}

export interface NamedRef {
  id: string
  name: string
}

export type EnrollmentStatus = 'Active' | 'Closed'

export interface EnrollmentListItem {
  id: string
  studentName: string
  instrumentName: string
  startDate: string
  status: EnrollmentStatus
}

export interface EnrollmentDetail {
  id: string
  student: NamedRef
  instrument: NamedRef
  startDate: string
  monthlyFee: number
  dueDay: number
  status: EnrollmentStatus
}

export interface EnrollmentRequest {
  studentId: string
  instrumentId: string
  startDate: string
  monthlyFee: number
  dueDay: number
}

export interface CloseEnrollmentRequest {
  endDate: string | null
  reason: string | null
}

export interface CloseEnrollmentResult {
  id: string
  status: EnrollmentStatus
  endDate: string
}

export type LessonStatus = 'Scheduled' | 'Completed' | 'Cancelled'

export interface LessonListItem {
  id: string
  seriesId: string | null
  studentName: string
  teacherName: string
  instrument: string
  startsAt: string
  endsAt: string
  room: string
  status: LessonStatus
}

export interface LessonDetail {
  id: string
  seriesId: string | null
  enrollmentId: string
  teacher: NamedRef
  student: NamedRef
  instrument: string
  startsAt: string
  endsAt: string
  room: string
  status: LessonStatus
  reason: string | null
  cancelledAt: string | null
}

export interface LessonRequest {
  enrollmentId: string
  teacherId: string
  startsAt: string
  durationMinutes: number
  room: string
}

export type Weekday = 'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday' | 'Sunday'

export interface RecurringLessonRequest {
  enrollmentId: string
  teacherId: string
  weekday: Weekday
  time: string
  durationMinutes: number
  room: string
  startDate: string
  endDate: string
}

export interface RecurringLessonResult {
  seriesId: string
  lessonsCreated: number
  firstLesson: string
  lastLesson: string
}

export interface LessonConflicts {
  conflictingDates: string[]
}

export interface RescheduleLessonRequest {
  newStartsAt: string
  room: string
  reason: string
}

export interface CancelLessonRequest {
  reason: string
}

export type TuitionStatus = 'Pending' | 'Paid'

export type PaymentMethod = 'Pix' | 'Cash' | 'Card'

export interface TuitionListItem {
  id: string
  enrollmentId: string
  studentName: string
  instrumentName: string
  referenceMonth: string
  amount: number
  dueDate: string
  status: TuitionStatus
  paidAt: string | null
  paidAmount: number | null
  method: PaymentMethod | null
}

export interface GenerateTuitionsResult {
  referenceMonth: string
  created: number
  skipped: number
}

export interface RegisterPaymentRequest {
  paidAt: string | null
  amount: number | null
  method: PaymentMethod | null
}
