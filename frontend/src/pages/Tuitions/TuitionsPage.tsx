import { useState } from 'react'
import { CircleDollarSign, Plus } from 'lucide-react'
import {
  Badge,
  Button,
  DataTable,
  FilterBar,
  IconButton,
  Input,
  PageHeader,
  Pagination,
  Select,
  type Column,
} from '@/components/ui'
import { optionsFrom, paymentMethods, tuitionStatus } from '@/config/labels'
import { useStudentOptions } from '@/hooks/useLookups'
import { usePagedQuery } from '@/hooks/usePagedQuery'
import { tuitionService, type TuitionListParams } from '@/services/tuitionService'
import type { TuitionListItem, TuitionStatus } from '@/types/entities'
import { formatCurrency, formatDate, formatReferenceMonth } from '@/utils/format'
import { brMonthToIso, maskMonth } from '@/utils/masks'
import listStyles from '../ListPage.module.css'
import styles from './TuitionsPage.module.css'
import { GenerateTuitionsModal } from './GenerateTuitionsModal'
import { RegisterPaymentModal } from './RegisterPaymentModal'

type Filters = Omit<TuitionListParams, 'page' | 'pageSize'>

type ModalState = { kind: 'generate' } | { kind: 'payment'; tuition: TuitionListItem }

const statusOptions = [{ value: '', label: 'Todos' }, ...optionsFrom(tuitionStatus)]

function PaymentInfo({ tuition }: { tuition: TuitionListItem }) {
  if (!tuition.paidAt) return <span className={styles.empty}>—</span>

  return (
    <span className={styles.payment}>
      <span>{formatDate(tuition.paidAt)}</span>
      <span className={styles.paymentMeta}>
        {tuition.method ? paymentMethods[tuition.method] : '—'} · {formatCurrency(tuition.paidAmount ?? 0)}
      </span>
    </span>
  )
}

export function TuitionsPage() {
  const list = usePagedQuery<TuitionListItem, Filters>(tuitionService.list, {})
  const studentOptions = useStudentOptions('Todos')
  const [monthText, setMonthText] = useState('')
  const [modal, setModal] = useState<ModalState | null>(null)
  const closeModal = () => setModal(null)

  function handleMonthChange(value: string) {
    const masked = maskMonth(value)
    setMonthText(masked)

    if (masked === '') list.setFilter('referenceMonth', undefined)
    else if (masked.length === 7) list.setFilter('referenceMonth', brMonthToIso(masked))
  }

  const columns: Column<TuitionListItem>[] = [
    { key: 'student', header: 'Aluno', render: (row) => row.studentName },
    { key: 'instrument', header: 'Instrumento', render: (row) => row.instrumentName, muted: true },
    { key: 'month', header: 'Mês', render: (row) => formatReferenceMonth(row.referenceMonth) },
    { key: 'amount', header: 'Valor', render: (row) => formatCurrency(row.amount) },
    { key: 'dueDate', header: 'Vencimento', render: (row) => formatDate(row.dueDate) },
    {
      key: 'status',
      header: 'Status',
      render: (row) => <Badge tone={tuitionStatus[row.status].tone}>{tuitionStatus[row.status].label}</Badge>,
    },
    { key: 'payment', header: 'Pagamento', render: (row) => <PaymentInfo tuition={row} /> },
    {
      key: 'actions',
      header: '',
      align: 'right',
      render: (row) =>
        row.status === 'Pending' && (
          <span className={listStyles.actions}>
            <IconButton
              icon={CircleDollarSign}
              label={`Registrar pagamento de ${row.studentName}`}
              onClick={() => setModal({ kind: 'payment', tuition: row })}
            />
          </span>
        ),
    },
  ]

  return (
    <>
      <PageHeader
        title="Mensalidades"
        description="Cobranças e pagamentos"
        actions={
          <Button icon={Plus} onClick={() => setModal({ kind: 'generate' })}>
            Gerar mensalidades
          </Button>
        }
      />

      <FilterBar>
        <Input
          label="Mês de referência"
          placeholder="mm/aaaa"
          inputMode="numeric"
          value={monthText}
          onChange={(event) => handleMonthChange(event.target.value)}
        />
        <Select
          label="Status"
          options={statusOptions}
          value={list.filters.status ?? ''}
          onChange={(event) => list.setFilter('status', (event.target.value || undefined) as TuitionStatus | undefined)}
        />
        <Select
          label="Aluno"
          options={studentOptions}
          value={list.filters.studentId ?? ''}
          onChange={(event) => list.setFilter('studentId', event.target.value || undefined)}
        />
      </FilterBar>

      <DataTable
        columns={columns}
        rows={list.items}
        rowKey={(row) => row.id}
        loading={list.loading}
        error={list.error}
        emptyMessage="Nenhuma mensalidade encontrada"
        footer={
          <Pagination page={list.page} pageSize={list.pageSize} total={list.total} onPageChange={list.setPage} />
        }
      />

      {modal?.kind === 'generate' && <GenerateTuitionsModal onClose={closeModal} onSaved={list.reload} />}

      {modal?.kind === 'payment' && (
        <RegisterPaymentModal tuition={modal.tuition} onClose={closeModal} onSaved={list.reload} />
      )}
    </>
  )
}
