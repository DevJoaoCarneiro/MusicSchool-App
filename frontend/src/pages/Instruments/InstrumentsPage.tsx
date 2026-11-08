import { useState } from 'react'
import { Pencil, Plus, Trash2 } from 'lucide-react'
import { Button, ConfirmDialog, DataTable, IconButton, PageHeader, type Column } from '@/components/ui'
import { hasRole } from '@/config/roles'
import { useApiQuery } from '@/hooks/useApiQuery'
import { useAuth } from '@/hooks/useAuth'
import { useToast } from '@/hooks/useToast'
import { instrumentService } from '@/services/instrumentService'
import type { Instrument } from '@/types/entities'
import listStyles from '../ListPage.module.css'
import { InstrumentFormModal } from './InstrumentFormModal'

type FormTarget = { mode: 'create' } | { mode: 'edit'; instrument: Instrument }

export function InstrumentsPage() {
  const { user } = useAuth()
  const { showToast } = useToast()
  const { data, loading, error, reload } = useApiQuery(instrumentService.list)
  const [formTarget, setFormTarget] = useState<FormTarget | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<Instrument | null>(null)
  const [deleting, setDeleting] = useState(false)
  const canManage = hasRole(user?.role, ['Admin'])

  async function handleDelete() {
    if (!deleteTarget) return

    setDeleting(true)
    const response = await instrumentService.remove(deleteTarget.id)
    setDeleting(false)
    setDeleteTarget(null)

    showToast(response.message, response.status === 'Success' ? 'success' : 'error')
    if (response.status === 'Success') reload()
  }

  const columns: Column<Instrument>[] = [
    { key: 'name', header: 'Nome', render: (row) => row.name, width: '27%' },
    { key: 'description', header: 'Descrição', render: (row) => row.description, muted: true },
  ]

  if (canManage) {
    columns.push({
      key: 'actions',
      header: '',
      align: 'right',
      render: (row) => (
        <span className={listStyles.actions}>
          <IconButton
            icon={Pencil}
            label={`Editar ${row.name}`}
            onClick={() => setFormTarget({ mode: 'edit', instrument: row })}
          />
          <IconButton icon={Trash2} label={`Excluir ${row.name}`} tone="danger" onClick={() => setDeleteTarget(row)} />
        </span>
      ),
    })
  }

  return (
    <>
      <PageHeader
        title="Instrumentos"
        description="Instrumentos ensinados na escola"
        actions={
          canManage && (
            <Button icon={Plus} onClick={() => setFormTarget({ mode: 'create' })}>
              Novo instrumento
            </Button>
          )
        }
      />

      <DataTable
        columns={columns}
        rows={data ?? []}
        rowKey={(row) => row.id}
        loading={loading}
        error={error}
        emptyMessage="Nenhum instrumento cadastrado"
      />

      {formTarget && (
        <InstrumentFormModal
          instrument={formTarget.mode === 'edit' ? formTarget.instrument : undefined}
          onClose={() => setFormTarget(null)}
          onSaved={reload}
        />
      )}

      <ConfirmDialog
        open={deleteTarget !== null}
        title="Excluir instrumento"
        message={`Tem certeza que deseja excluir "${deleteTarget?.name ?? ''}"?`}
        loading={deleting}
        onConfirm={handleDelete}
        onClose={() => setDeleteTarget(null)}
      />
    </>
  )
}
