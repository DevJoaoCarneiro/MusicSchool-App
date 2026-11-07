import { accounts } from '@/mocks/db'
import { emailTaken, fail, newId, paginate, respond } from '@/mocks/helpers'
import type { ApiResponse, PagedResult, PageParams } from '@/types/api'
import type { UserListItem, UserRequest } from '@/types/entities'

function toListItem({ id, name, email, role }: UserListItem): UserListItem {
  return { id, name, email, role }
}

export const userService = {
  async list(params: PageParams): Promise<ApiResponse<PagedResult<UserListItem>>> {
    return respond(paginate(accounts.map(toListItem), params))
  },

  async create(request: UserRequest): Promise<ApiResponse<UserListItem>> {
    if (emailTaken(accounts, request.email)) {
      return fail('conflict', 'E-mail já cadastrado')
    }

    const account = { ...request, id: newId('u'), email: request.email.trim().toLowerCase() }
    accounts.push(account)
    return respond(toListItem(account), 'Usuário criado com sucesso')
  },
}
