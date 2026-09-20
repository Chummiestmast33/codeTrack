import { Navigate } from 'react-router'
import { useAuth } from '../features/auth/AuthContext.jsx'

/** Role-aware landing for `/`: admins go to /admin, students to /app. */
export default function RoleLanding() {
  const { token, role, loading } = useAuth()

  if (loading) return null
  if (!token) return <Navigate to="/login" replace />
  if (role === 'Administrator') return <Navigate to="/admin/users" replace />
  return <Navigate to="/app" replace />
}
