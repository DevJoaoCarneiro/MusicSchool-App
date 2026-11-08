import { useCallback } from 'react'
import { Button, DetailList, Modal, TagList } from '@/components/ui'
import { useApiQuery } from '@/hooks/useApiQuery'
import { teacherService } from '@/services/teacherService'
import { formatCpf, formatPhone } from '@/utils/format'
import modalStyles from '../ModalContent.module.css'

interface TeacherDetailModalProps {
  teacherId: string
  canEdit: boolean
  onEdit: () => void
  onClose: () => void
}

export function TeacherDetailModal({ teacherId, canEdit, onEdit, onClose }: TeacherDetailModalProps) {
  const fetcher = useCallback(() => teacherService.getById(teacherId), [teacherId])
  const { data: teacher, loading, error } = useApiQuery(fetcher)

  return (
    <Modal
      open
      title="Detalhes do professor"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Fechar
          </Button>
          {canEdit && teacher && <Button onClick={onEdit}>Editar</Button>}
        </>
      }
    >
      {loading && <p className={modalStyles.loading}>Carregando…</p>}
      {error && <p className={modalStyles.loading}>{error}</p>}
      {teacher && (
        <DetailList
          items={[
            { label: 'Nome', value: teacher.name, wide: true },
            { label: 'E-mail', value: teacher.email, wide: true },
            { label: 'Telefone', value: formatPhone(teacher.phone) },
            { label: 'CPF', value: formatCpf(teacher.cpf) },
            { label: 'Especialidades', value: <TagList items={teacher.specialties} />, wide: true },
          ]}
        />
      )}
    </Modal>
  )
}
