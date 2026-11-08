import { useState } from 'react'
import { Eye, Pencil, Plus, Trash2 } from 'lucide-react'
import {
  Button,
  ConfirmDialog,
  DataTable,
  IconButton,
  PageHeader,
  Pagination,
  SearchInput,
  type Column,
} from '@/components/ui'
import { hasRole, managerRoles } from '@/config/roles'
import { useAuth } from '@/hooks/useAuth'
import { usePagedList } from '@/hooks/usePagedList'
import { useToast } from '@/hooks/useToast'
import { studentService } from '@/services/studentService'
import type { StudentListItem } from '@/types/entities'
import { formatDate, formatPhone } from '@/utils/format'
import listStyles from '../ListPage.module.css'
import { StudentDetailModal } from './StudentDetailModal'
import { StudentFormModal } from './StudentFormModal'

type ModalState =
  | { kind: 'create' }
  | { kind: 'edit'; id: string }
  | { kind: 'detail'; id: string }
  | { kind: 'delete'; student: StudentListItem }

export function StudentsPage() {
  const { user } = useAuth()
  const { showToast } = useToast()
  const list = usePagedList(studentService.list)
  const [modal, setModal] = useState<ModalState | null>(null)
  const [deleting, setDeleting] = useState(false)
  const canManage = hasRole(user?.role, managerRoles)
  const closeModal = () => setModal(null)

  async function handleDelete(student: StudentListItem) {
    setDeleting(true)
    const response = await studentService.remove(student.id)
    setDeleting(false)
    setModal(null)

    showToast(response.message, response.status === 'Success' ? 'success' : 'error')
    if (response.status === 'Success') list.reload()
  }

  const columns: Column<StudentListItem>[] = [
    { key: 'name', header: 'Nome', render: (row) => row.name },
    { key: 'email', header: 'E-mail', render: (row) => row.email, muted: true },
    { key: 'phone', header: 'Telefone', render: (row) => formatPhone(row.phone) },
    { key: 'birthDate', header: 'Nascimento', render: (row) => formatDate(row.birthDate) },
  ]

  if (canManage) {
    columns.push({
      key: 'actions',
      header: '',
      align: 'right',
      render: (row) => (
        <span className={listStyles.actions}>
          <IconButton icon={Eye} label={`Ver ${row.name}`} onClick={() => setModal({ kind: 'detail', id: row.id })} />
          <IconButton icon={Pencil} label={`Editar ${row.name}`} onClick={() => setModal({ kind: 'edit', id: row.id })} />
          <IconButton
            icon={Trash2}
            label={`Excluir ${row.name}`}
            tone="danger"
            onClick={() => setModal({ kind: 'delete', student: row })}
          />
        </span>
      ),
    })
  }

  return (
    <>
      <PageHeader
        title="Alunos"
        actions={
          canManage && (
            <Button icon={Plus} onClick={() => setModal({ kind: 'create' })}>
              Novo aluno
            </Button>
          )
        }
      />

      <div className={listStyles.toolbar}>
        <SearchInput
          label="Buscar alunos por nome"
          placeholder="Buscar por nome..."
          value={list.search}
          onChange={(event) => list.setSearch(event.target.value)}
        />
      </div>

      <DataTable
        columns={columns}
        rows={list.items}
        rowKey={(row) => row.id}
        loading={list.loading}
        error={list.error}
        emptyMessage="Nenhum aluno encontrado"
        footer={
          <Pagination page={list.page} pageSize={list.pageSize} total={list.total} onPageChange={list.setPage} />
        }
      />

      {modal?.kind === 'create' && <StudentFormModal onClose={closeModal} onSaved={list.reload} />}

      {modal?.kind === 'edit' && <StudentFormModal studentId={modal.id} onClose={closeModal} onSaved={list.reload} />}

      {modal?.kind === 'detail' && (
        <StudentDetailModal
          studentId={modal.id}
          canEdit={canManage}
          onEdit={() => setModal({ kind: 'edit', id: modal.id })}
          onClose={closeModal}
        />
      )}

      <ConfirmDialog
        open={modal?.kind === 'delete'}
        title="Excluir aluno"
        message={`Tem certeza que deseja excluir "${modal?.kind === 'delete' ? modal.student.name : ''}"?`}
        loading={deleting}
        onConfirm={() => modal?.kind === 'delete' && handleDelete(modal.student)}
        onClose={closeModal}
      />
    </>
  )
}
