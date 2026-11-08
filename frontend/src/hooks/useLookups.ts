import type { SelectOption } from '@/components/ui'
import { enrollmentService } from '@/services/enrollmentService'
import { instrumentService } from '@/services/instrumentService'
import { studentService } from '@/services/studentService'
import { teacherService } from '@/services/teacherService'
import { useApiQuery } from './useApiQuery'

const maxPageSize = 100

const fetchStudents = () => studentService.list({ page: 1, pageSize: maxPageSize })
const fetchTeachers = () => teacherService.list({ page: 1, pageSize: maxPageSize })
const fetchActiveEnrollments = () => enrollmentService.list({ status: 'Active', page: 1, pageSize: maxPageSize })

function withPlaceholder(options: SelectOption[], placeholder?: string) {
  return placeholder ? [{ value: '', label: placeholder }, ...options] : options
}

export function useStudentOptions(placeholder?: string) {
  const { data } = useApiQuery(fetchStudents)
  return withPlaceholder((data?.items ?? []).map((item) => ({ value: item.id, label: item.name })), placeholder)
}

export function useTeacherOptions(placeholder?: string) {
  const { data } = useApiQuery(fetchTeachers)
  return withPlaceholder((data?.items ?? []).map((item) => ({ value: item.id, label: item.name })), placeholder)
}

export function useInstrumentOptions(placeholder?: string) {
  const { data } = useApiQuery(instrumentService.list)
  return withPlaceholder((data ?? []).map((item) => ({ value: item.id, label: item.name })), placeholder)
}

export function useActiveEnrollmentOptions(placeholder?: string) {
  const { data } = useApiQuery(fetchActiveEnrollments)
  return withPlaceholder(
    (data?.items ?? []).map((item) => ({ value: item.id, label: `${item.studentName} — ${item.instrumentName}` })),
    placeholder,
  )
}
