import { useCallback } from 'react'
import { Badge, Button, DetailList, Modal } from '@/components/ui'
import { enrollmentStatus } from '@/config/labels'
import { useApiQuery } from '@/hooks/useApiQuery'
import { enrollmentService } from '@/services/enrollmentService'
import { formatCurrency, formatDate } from '@/utils/format'
import modalStyles from '../ModalContent.module.css'

interface EnrollmentDetailModalProps {
  enrollmentId: string
  canManage: boolean
  onCloseEnrollment: (label: string) => void
  onClose: () => void
}

export function EnrollmentDetailModal({
  enrollmentId,
  canManage,
  onCloseEnrollment,
  onClose,
}: EnrollmentDetailModalProps) {
  const fetcher = useCallback(() => enrollmentService.getById(enrollmentId), [enrollmentId])
  const { data: enrollment, loading, error } = useApiQuery(fetcher)

  return (
    <Modal
      open
      title="Detalhes da matrícula"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Fechar
          </Button>
          {canManage && enrollment?.status === 'Active' && (
            <Button
              variant="danger"
              onClick={() => onCloseEnrollment(`${enrollment.student.name} — ${enrollment.instrument.name}`)}
            >
              Encerrar matrícula
            </Button>
          )}
        </>
      }
    >
      {loading && <p className={modalStyles.loading}>Carregando…</p>}
      {error && <p className={modalStyles.loading}>{error}</p>}
      {enrollment && (
        <DetailList
          items={[
            { label: 'Aluno', value: enrollment.student.name },
            { label: 'Instrumento', value: enrollment.instrument.name },
            { label: 'Início', value: formatDate(enrollment.startDate) },
            {
              label: 'Status',
              value: (
                <Badge tone={enrollmentStatus[enrollment.status].tone}>
                  {enrollmentStatus[enrollment.status].label}
                </Badge>
              ),
            },
            { label: 'Mensalidade', value: formatCurrency(enrollment.monthlyFee) },
            { label: 'Vencimento', value: `Todo dia ${enrollment.dueDay}` },
          ]}
        />
      )}
    </Modal>
  )
}
