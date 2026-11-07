import { accounts } from '@/mocks/db'
import { fail, respond } from '@/mocks/helpers'
import type { ApiResponse } from '@/types/api'
import type { AuthSession, ChangePasswordRequest, LoginRequest } from '@/types/entities'

export const authService = {
  async login({ email, password }: LoginRequest): Promise<ApiResponse<AuthSession>> {
    const account = accounts.find((item) => item.email === email.trim().toLowerCase())

    if (!account || account.password !== password) {
      return fail('unauthorized', 'E-mail ou senha inválidos')
    }

    return respond<AuthSession>(
      {
        token: 'mock-token',
        expiresIn: 3600,
        user: { id: account.id, name: account.name, email: account.email, role: account.role },
      },
      'Login realizado com sucesso',
    )
  },

  async changePassword(
    userId: string,
    { currentPassword, newPassword }: ChangePasswordRequest,
  ): Promise<ApiResponse<null>> {
    const account = accounts.find((item) => item.id === userId)

    if (!account || account.password !== currentPassword) {
      return fail('invalid_argument', 'Senha atual incorreta')
    }

    if (newPassword.length < 6) {
      return fail('invalid_argument', 'A nova senha deve ter pelo menos 6 caracteres')
    }

    account.password = newPassword
    return respond(null, 'Senha alterada com sucesso')
  },
}
