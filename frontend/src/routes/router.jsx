import { createBrowserRouter, Navigate, RouterProvider } from 'react-router'
import { AdminLayout, AppLayout } from '../components/Layouts.jsx'
import { PublicOnly, RequireAdmin, RequireApproved, RequireAuth } from './guards.jsx'
import Login from '../pages/Login.jsx'
import Register from '../pages/Register.jsx'
import Pending from '../pages/Pending.jsx'
import Dashboard from '../pages/Dashboard.jsx'
import Profile from '../pages/Profile.jsx'
import NotFound from '../pages/NotFound.jsx'
import UsersPage from '../features/admin/UsersPage.jsx'
import TopicsPage from '../features/admin/TopicsPage.jsx'
import SessionsPage from '../features/admin/SessionsPage.jsx'
import SessionDetailPage from '../features/admin/SessionDetailPage.jsx'
import StudentSessionsPage from '../features/student/SessionsPage.jsx'
import AttendPage from '../features/student/AttendPage.jsx'

const router = createBrowserRouter([
  { path: '/', element: <Navigate to="/app" replace /> },
  {
    path: '/login',
    element: (
      <PublicOnly>
        <Login />
      </PublicOnly>
    ),
  },
  {
    path: '/register',
    element: (
      <PublicOnly>
        <Register />
      </PublicOnly>
    ),
  },
  { path: '/pending', element: <Pending /> },
  {
    path: '/app',
    element: (
      <RequireAuth>
        <RequireApproved>
          <AppLayout />
        </RequireApproved>
      </RequireAuth>
    ),
    children: [
      { index: true, element: <Dashboard /> },
      { path: 'me', element: <Profile /> },
      { path: 'sessions', element: <StudentSessionsPage /> },
      { path: 'attend', element: <AttendPage /> },
      { path: 'attend/:token', element: <AttendPage /> },
      { path: '*', element: <NotFound /> },
    ],
  },
  {
    path: '/admin',
    element: (
      <RequireAuth>
        <RequireAdmin>
          <AdminLayout />
        </RequireAdmin>
      </RequireAuth>
    ),
    children: [
      { index: true, element: <Navigate to="users" replace /> },
      { path: 'users', element: <UsersPage /> },
      { path: 'topics', element: <TopicsPage /> },
      { path: 'sessions', element: <SessionsPage /> },
      { path: 'sessions/:id', element: <SessionDetailPage /> },
      { path: '*', element: <NotFound /> },
    ],
  },
  { path: '/404', element: <NotFound /> },
  { path: '*', element: <NotFound /> },
])

export default function AppRouter() {
  return <RouterProvider router={router} />
}
