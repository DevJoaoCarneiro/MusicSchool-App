import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { AppLayout } from '@/components/layout'
import { AgendaPage } from '@/pages/Agenda/AgendaPage'
import { ChangePasswordPage } from '@/pages/ChangePassword/ChangePasswordPage'
import { EnrollmentsPage } from '@/pages/Enrollments/EnrollmentsPage'
import { HomePage } from '@/pages/Home/HomePage'
import { InstrumentsPage } from '@/pages/Instruments/InstrumentsPage'
import { LoginPage } from '@/pages/Login/LoginPage'
import { NoAccessPage } from '@/pages/NoAccess/NoAccessPage'
import { NotFoundPage } from '@/pages/NotFound/NotFoundPage'
import { StudentsPage } from '@/pages/Students/StudentsPage'
import { TeachersPage } from '@/pages/Teachers/TeachersPage'
import { TuitionsPage } from '@/pages/Tuitions/TuitionsPage'
import { UsersPage } from '@/pages/Users/UsersPage'
import { managerRoles } from '@/config/roles'
import { GuestOnly, RequireAuth, RequireRole } from './guards'

const router = createBrowserRouter([
  {
    path: '/login',
    element: (
      <GuestOnly>
        <LoginPage />
      </GuestOnly>
    ),
  },
  {
    path: '/sem-acesso',
    element: <NoAccessPage />,
  },
  {
    path: '/',
    element: (
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    children: [
      { index: true, element: <HomePage /> },
      { path: 'alunos', element: <StudentsPage /> },
      { path: 'professores', element: <TeachersPage /> },
      { path: 'instrumentos', element: <InstrumentsPage /> },
      { path: 'matriculas', element: <EnrollmentsPage /> },
      { path: 'agenda', element: <AgendaPage /> },
      {
        path: 'mensalidades',
        element: (
          <RequireRole roles={managerRoles}>
            <TuitionsPage />
          </RequireRole>
        ),
      },
      {
        path: 'usuarios',
        element: (
          <RequireRole roles={['Admin']}>
            <UsersPage />
          </RequireRole>
        ),
      },
      { path: 'trocar-senha', element: <ChangePasswordPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])

export function AppRouter() {
  return <RouterProvider router={router} />
}
