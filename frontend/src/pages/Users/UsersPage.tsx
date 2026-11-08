import { useState } from 'react'
import { Plus } from 'lucide-react'
import { Button, DataTable, PageHeader, Pagination, Tag, type Column } from '@/components/ui'
import { roleLabels } from '@/config/roles'
import { usePagedList } from '@/hooks/usePagedList'
import { userService } from '@/services/userService'
import type { UserListItem } from '@/types/entities'
import { UserFormModal } from './UserFormModal'

const columns: Column<UserListItem>[] = [
  { key: 'name', header: 'Nome', render: (row) => row.name, width: '27%' },
  { key: 'email', header: 'E-mail', render: (row) => row.email, muted: true, width: '44%' },
  { key: 'role', header: 'Perfil', render: (row) => <Tag>{roleLabels[row.role]}</Tag> },
]

export function UsersPage() {
  const list = usePagedList(userService.list)
  const [formOpen, setFormOpen] = useState(false)

  return (
    <>
      <PageHeader
        title="Usuários"
        description="Contas de acesso ao sistema"
        actions={
          <Button icon={Plus} onClick={() => setFormOpen(true)}>
            Novo usuário
          </Button>
        }
      />

      <DataTable
        columns={columns}
        rows={list.items}
        rowKey={(row) => row.id}
        loading={list.loading}
        error={list.error}
        emptyMessage="Nenhum usuário encontrado"
        footer={
          <Pagination page={list.page} pageSize={list.pageSize} total={list.total} onPageChange={list.setPage} />
        }
      />

      {formOpen && <UserFormModal onClose={() => setFormOpen(false)} onSaved={list.reload} />}
    </>
  )
}
