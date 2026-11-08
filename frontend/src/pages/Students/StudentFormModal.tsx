import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Button, FormRow, FormSection, Input, Modal } from '@/components/ui'
import { useToast } from '@/hooks/useToast'
import { studentService } from '@/services/studentService'
import type { StudentDetail, StudentRequest } from '@/types/entities'
import {
  brDateToIso,
  isoToBrDate,
  maskCep,
  maskCpf,
  maskDate,
  maskPhone,
  maskUf,
  onlyDigits,
} from '@/utils/masks'
import modalStyles from '../ModalContent.module.css'

interface StudentFormModalProps {
  studentId?: string
  onClose: () => void
  onSaved: () => void
}

const emptyForm = {
  name: '',
  email: '',
  phone: '',
  birthDate: '',
  cpf: '',
  street: '',
  number: '',
  city: '',
  state: '',
  zipCode: '',
  guardianName: '',
  guardianPhone: '',
  guardianEmail: '',
  guardianCpf: '',
}

type FormState = typeof emptyForm

function fromDetail(student: StudentDetail): FormState {
  return {
    name: student.name,
    email: student.email,
    phone: maskPhone(student.phone),
    birthDate: isoToBrDate(student.birthDate),
    cpf: maskCpf(student.cpf),
    street: student.address.street,
    number: student.address.number,
    city: student.address.city,
    state: student.address.state,
    zipCode: maskCep(student.address.zipCode),
    guardianName: student.guardian?.name ?? '',
    guardianPhone: maskPhone(student.guardian?.phone ?? ''),
    guardianEmail: student.guardian?.email ?? '',
    guardianCpf: maskCpf(student.guardian?.cpf ?? ''),
  }
}

function toRequest(form: FormState): StudentRequest {
  const hasGuardian = [form.guardianName, form.guardianPhone, form.guardianEmail, form.guardianCpf].some(
    (value) => value.trim() !== '',
  )

  return {
    name: form.name,
    email: form.email,
    phone: onlyDigits(form.phone),
    cpf: onlyDigits(form.cpf),
    birthDate: brDateToIso(form.birthDate),
    address: {
      street: form.street,
      number: form.number,
      city: form.city,
      state: form.state,
      zipCode: onlyDigits(form.zipCode),
    },
    guardian: hasGuardian
      ? {
          name: form.guardianName,
          phone: onlyDigits(form.guardianPhone),
          email: form.guardianEmail,
          cpf: onlyDigits(form.guardianCpf),
        }
      : null,
  }
}

const formId = 'student-form'

export function StudentFormModal({ studentId, onClose, onSaved }: StudentFormModalProps) {
  const { showToast } = useToast()
  const [form, setForm] = useState<FormState>(emptyForm)
  const [loading, setLoading] = useState(Boolean(studentId))
  const [submitting, setSubmitting] = useState(false)
  const editing = Boolean(studentId)
  const closeRef = useRef(onClose)

  useEffect(() => {
    closeRef.current = onClose
  }, [onClose])

  useEffect(() => {
    if (!studentId) return

    let active = true
    studentService.getById(studentId).then((response) => {
      if (!active) return

      if (response.status === 'Success' && response.data) {
        setForm(fromDetail(response.data))
        setLoading(false)
      } else {
        showToast(response.message, 'error')
        closeRef.current()
      }
    })

    return () => {
      active = false
    }
  }, [studentId, showToast])

  function field<K extends keyof FormState>(key: K, mask?: (value: string) => string) {
    return {
      value: form[key],
      onChange: (event: { target: { value: string } }) => {
        const value = mask ? mask(event.target.value) : event.target.value
        setForm((current) => ({ ...current, [key]: value }))
      },
    }
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitting(true)
    const request = toRequest(form)
    const response = studentId
      ? await studentService.update(studentId, request)
      : await studentService.create(request)
    setSubmitting(false)

    if (response.status !== 'Success') {
      showToast(response.message, 'error')
      return
    }

    showToast(response.message)
    onSaved()
    onClose()
  }

  return (
    <Modal
      open
      title={editing ? 'Editar aluno' : 'Novo aluno'}
      size="lg"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={submitting}>
            Cancelar
          </Button>
          <Button type="submit" form={formId} loading={submitting} disabled={loading}>
            Salvar
          </Button>
        </>
      }
    >
      {loading ? (
        <p className={modalStyles.loading}>Carregando…</p>
      ) : (
        <form id={formId} onSubmit={handleSubmit}>
          <FormSection title="Dados pessoais">
            <Input label="Nome" autoFocus {...field('name')} />
            <FormRow>
              <Input label="E-mail" type="email" {...field('email')} />
              <Input label="Telefone" placeholder="(00) 00000-0000" inputMode="tel" {...field('phone', maskPhone)} />
            </FormRow>
            <FormRow>
              <Input label="Nascimento" placeholder="dd/mm/aaaa" inputMode="numeric" {...field('birthDate', maskDate)} />
              <Input label="CPF" placeholder="000.000.000-00" inputMode="numeric" {...field('cpf', maskCpf)} />
            </FormRow>
          </FormSection>

          <FormSection title="Endereço">
            <FormRow columns="2fr 1fr">
              <Input label="Rua" {...field('street')} />
              <Input label="Número" {...field('number')} />
            </FormRow>
            <FormRow columns="2fr 0.6fr 1fr">
              <Input label="Cidade" {...field('city')} />
              <Input label="UF" {...field('state', maskUf)} />
              <Input label="CEP" placeholder="00000-000" inputMode="numeric" {...field('zipCode', maskCep)} />
            </FormRow>
          </FormSection>

          <FormSection title="Responsável" hint="opcional">
            <Input label="Nome" {...field('guardianName')} />
            <FormRow columns="1fr 1fr 1fr">
              <Input label="Telefone" inputMode="tel" {...field('guardianPhone', maskPhone)} />
              <Input label="E-mail" type="email" {...field('guardianEmail')} />
              <Input label="CPF" inputMode="numeric" {...field('guardianCpf', maskCpf)} />
            </FormRow>
          </FormSection>
        </form>
      )}
    </Modal>
  )
}
