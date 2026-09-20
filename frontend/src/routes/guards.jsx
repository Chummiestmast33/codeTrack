import { Navigate, useLocation } from 'react-router'
import { useAuth } from '../features/auth/AuthContext.jsx'

export function RequireAuth({ children }) {
  const { token, loading } = useAuth()
  const location = useLocation()

  if (loading) return null
  if (!token) return <Navigate to="/login" replace state={{ from: location.pathname }} />
  return children
}

export function RequireApproved({ children }) {
  const { user, loading } = useAuth()
  const location = useLocation()

  if (loading || !user) return null
  if (!(user.approvalStatus === 'Approved' && user.isActive === true)) {
    return <Navigate to="/pending" replace state={{ from: location.pathname }} />
  }
  return children
}

export function RequireAdmin({ children }) {
  const { role, loading } = useAuth()

  if (loading) return null
  if (role !== 'Administrator') return <Navigate to="/404" replace />
  return children
}

export function RequireStudent({ children }) {
  const { role, loading } = useAuth()

  if (loading) return null
  if (role === 'Administrator') return <Navigate to="/admin/users" replace />
  return children
}

export function PublicOnly({ children }) {
  const { token, user, loading } = useAuth()

  if (loading) return null
  if (token && user) {
    return <Navigate to={user.role === 'Administrator' ? '/admin/users' : '/app'} replace />
  }
  return children
}
