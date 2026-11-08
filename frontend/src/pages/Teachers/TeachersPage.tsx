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
  TagList,
  type Column,
} from '@/components/ui'
import { hasRole, managerRoles } from '@/config/roles'
import { useAuth } from '@/hooks/useAuth'
import { usePagedList } from '@/hooks/usePagedList'
import { useToast } from '@/hooks/useToast'
import { teacherService } from '@/services/teacherService'
import type { TeacherListItem } from '@/types/entities'
import listStyles from '../ListPage.module.css'
import { TeacherDetailModal } from './TeacherDetailModal'
import { TeacherFormModal } from './TeacherFormModal'

type ModalState =
  | { kind: 'create' }
  | { kind: 'edit'; id: string }
  | { kind: 'detail'; id: string }
  | { kind: 'delete'; teacher: TeacherListItem }

export function TeachersPage() {
  const { user } = useAuth()
  const { showToast } = useToast()
  const list = usePagedList(teacherService.list)
  const [modal, setModal] = useState<ModalState | null>(null)
  const [deleting, setDeleting] = useState(false)
  const canManage = hasRole(user?.role, managerRoles)
  const closeModal = () => setModal(null)

  async function handleDelete(teacher: TeacherListItem) {
    setDeleting(true)
    const response = await teacherService.remove(teacher.id)
    setDeleting(false)
    setModal(null)

    showToast(response.message, response.status === 'Success' ? 'success' : 'error')
    if (response.status === 'Success') list.reload()
  }

  const columns: Column<TeacherListItem>[] = [
    { key: 'name', header: 'Nome', render: (row) => row.name, width: '28%' },
    { key: 'email', header: 'E-mail', render: (row) => row.email, muted: true, width: '34%' },
    { key: 'specialties', header: 'Especialidades', render: (row) => <TagList items={row.specialties} /> },
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
            onClick={() => setModal({ kind: 'delete', teacher: row })}
          />
        </span>
      ),
    })
  }

  return (
    <>
      <PageHeader
        title="Professores"
        actions={
          canManage && (
            <Button icon={Plus} onClick={() => setModal({ kind: 'create' })}>
              Novo professor
            </Button>
          )
        }
      />

      <div className={listStyles.toolbar}>
        <SearchInput
          label="Buscar professores por nome"
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
        emptyMessage="Nenhum professor encontrado"
        footer={
          <Pagination page={list.page} pageSize={list.pageSize} total={list.total} onPageChange={list.setPage} />
        }
      />

      {modal?.kind === 'create' && <TeacherFormModal onClose={closeModal} onSaved={list.reload} />}

      {modal?.kind === 'edit' && <TeacherFormModal teacherId={modal.id} onClose={closeModal} onSaved={list.reload} />}

      {modal?.kind === 'detail' && (
        <TeacherDetailModal
          teacherId={modal.id}
          canEdit={canManage}
          onEdit={() => setModal({ kind: 'edit', id: modal.id })}
          onClose={closeModal}
        />
      )}

      <ConfirmDialog
        open={modal?.kind === 'delete'}
        title="Excluir professor"
        message={`Tem certeza que deseja excluir "${modal?.kind === 'delete' ? modal.teacher.name : ''}"?`}
        loading={deleting}
        onConfirm={() => modal?.kind === 'delete' && handleDelete(modal.teacher)}
        onClose={closeModal}
      />
    </>
  )
}
