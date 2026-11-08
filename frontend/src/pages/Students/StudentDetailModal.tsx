import { useCallback } from 'react'
import { Button, DetailList, Modal } from '@/components/ui'
import { useApiQuery } from '@/hooks/useApiQuery'
import { studentService } from '@/services/studentService'
import { formatCep, formatCpf, formatDate, formatPhone } from '@/utils/format'
import modalStyles from '../ModalContent.module.css'

interface StudentDetailModalProps {
  studentId: string
  canEdit: boolean
  onEdit: () => void
  onClose: () => void
}

export function StudentDetailModal({ studentId, canEdit, onEdit, onClose }: StudentDetailModalProps) {
  const fetcher = useCallback(() => studentService.getById(studentId), [studentId])
  const { data: student, loading, error } = useApiQuery(fetcher)

  return (
    <Modal
      open
      title="Detalhes do aluno"
      size="lg"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Fechar
          </Button>
          {canEdit && student && <Button onClick={onEdit}>Editar</Button>}
        </>
      }
    >
      {loading && <p className={modalStyles.loading}>Carregando…</p>}
      {error && <p className={modalStyles.loading}>{error}</p>}
      {student && (
        <>
          <section className={modalStyles.section}>
            <h3 className={modalStyles.sectionTitle}>Dados pessoais</h3>
            <DetailList
              items={[
                { label: 'Nome', value: student.name, wide: true },
                { label: 'E-mail', value: student.email },
                { label: 'Telefone', value: formatPhone(student.phone) },
                { label: 'Nascimento', value: formatDate(student.birthDate) },
                { label: 'CPF', value: formatCpf(student.cpf) },
              ]}
            />
          </section>

          <section className={modalStyles.section}>
            <h3 className={modalStyles.sectionTitle}>Endereço</h3>
            <DetailList
              items={[
                { label: 'Rua', value: `${student.address.street}, ${student.address.number}`, wide: true },
                { label: 'Cidade', value: `${student.address.city} / ${student.address.state}` },
                { label: 'CEP', value: formatCep(student.address.zipCode) },
              ]}
            />
          </section>

          <section className={modalStyles.section}>
            <h3 className={modalStyles.sectionTitle}>Responsável</h3>
            {student.guardian ? (
              <DetailList
                items={[
                  { label: 'Nome', value: student.guardian.name, wide: true },
                  { label: 'Telefone', value: formatPhone(student.guardian.phone) },
                  { label: 'E-mail', value: student.guardian.email },
                  { label: 'CPF', value: formatCpf(student.guardian.cpf) },
                ]}
              />
            ) : (
              <p className={modalStyles.notice}>Nenhum responsável cadastrado.</p>
            )}
          </section>
        </>
      )}
    </Modal>
  )
}
