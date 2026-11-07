import { useState } from 'react'
import { Ban, Eye, Plus } from 'lucide-react'
import {
  Badge,
  Button,
  DataTable,
  FilterBar,
  IconButton,
  PageHeader,
  Pagination,
  Select,
  type Column,
} from '@/components/ui'
import { enrollmentStatus, optionsFrom } from '@/config/labels'
import { hasRole, managerRoles } from '@/config/roles'
import { useAuth } from '@/hooks/useAuth'
import { useInstrumentOptions, useStudentOptions } from '@/hooks/useLookups'
import { usePagedQuery } from '@/hooks/usePagedQuery'
import { enrollmentService, type EnrollmentListParams } from '@/services/enrollmentService'
import type { EnrollmentListItem, EnrollmentStatus } from '@/types/entities'
import { formatDate } from '@/utils/format'
import listStyles from '../ListPage.module.css'
import { CloseEnrollmentModal } from './CloseEnrollmentModal'
import { EnrollmentDetailModal } from './EnrollmentDetailModal'
import { EnrollmentFormModal } from './EnrollmentFormModal'

type Filters = Omit<EnrollmentListParams, 'page' | 'pageSize'>

type ModalState =
  | { kind: 'create' }
  | { kind: 'detail'; id: string }
  | { kind: 'close'; id: string; label: string }

const statusOptions = [{ value: '', label: 'Todos' }, ...optionsFrom(enrollmentStatus)]

export function EnrollmentsPage() {
  const { user } = useAuth()
  const list = usePagedQuery<EnrollmentListItem, Filters>(enrollmentService.list, {})
  const studentOptions = useStudentOptions('Todos')
  const instrumentOptions = useInstrumentOptions('Todos')
  const [modal, setModal] = useState<ModalState | null>(null)
  const canManage = hasRole(user?.role, managerRoles)
  const closeModal = () => setModal(null)

  const columns: Column<EnrollmentListItem>[] = [
    { key: 'student', header: 'Aluno', render: (row) => row.studentName, width: '30%' },
    { key: 'instrument', header: 'Instrumento', render: (row) => row.instrumentName, muted: true },
    { key: 'startDate', header: 'Início', render: (row) => formatDate(row.startDate) },
    {
      key: 'status',
      header: 'Status',
      render: (row) => <Badge tone={enrollmentStatus[row.status].tone}>{enrollmentStatus[row.status].label}</Badge>,
    },
    {
      key: 'actions',
      header: '',
      align: 'right',
      render: (row) => (
        <span className={listStyles.actions}>
          <IconButton
            icon={Eye}
            label={`Ver matrícula de ${row.studentName}`}
            onClick={() => setModal({ kind: 'detail', id: row.id })}
          />
          {canManage && row.status === 'Active' && (
            <IconButton
              icon={Ban}
              label={`Encerrar matrícula de ${row.studentName}`}
              tone="danger"
              onClick={() => setModal({ kind: 'close', id: row.id, label: `${row.studentName} — ${row.instrumentName}` })}
            />
          )}
        </span>
      ),
    },
  ]

  return (
    <>
      <PageHeader
        title="Matrículas"
        description="Alunos matriculados por instrumento"
        actions={
          canManage && (
            <Button icon={Plus} onClick={() => setModal({ kind: 'create' })}>
              Nova matrícula
            </Button>
          )
        }
      />

      <FilterBar>
        <Select
          label="Aluno"
          options={studentOptions}
          value={list.filters.studentId ?? ''}
          onChange={(event) => list.setFilter('studentId', event.target.value || undefined)}
        />
        <Select
          label="Instrumento"
          options={instrumentOptions}
          value={list.filters.instrumentId ?? ''}
          onChange={(event) => list.setFilter('instrumentId', event.target.value || undefined)}
        />
        <Select
          label="Status"
          options={statusOptions}
          value={list.filters.status ?? ''}
          onChange={(event) => list.setFilter('status', (event.target.value || undefined) as EnrollmentStatus | undefined)}
        />
      </FilterBar>

      <DataTable
        columns={columns}
        rows={list.items}
        rowKey={(row) => row.id}
        loading={list.loading}
        error={list.error}
        emptyMessage="Nenhuma matrícula encontrada"
        footer={
          <Pagination page={list.page} pageSize={list.pageSize} total={list.total} onPageChange={list.setPage} />
        }
      />

      {modal?.kind === 'create' && <EnrollmentFormModal onClose={closeModal} onSaved={list.reload} />}

      {modal?.kind === 'detail' && (
        <EnrollmentDetailModal
          enrollmentId={modal.id}
          canManage={canManage}
          onCloseEnrollment={(label) => setModal({ kind: 'close', id: modal.id, label })}
          onClose={closeModal}
        />
      )}

      {modal?.kind === 'close' && (
        <CloseEnrollmentModal
          enrollmentId={modal.id}
          label={modal.label}
          onClose={closeModal}
          onSaved={list.reload}
        />
      )}
    </>
  )
}
