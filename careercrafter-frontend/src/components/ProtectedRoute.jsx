import { Navigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function ProtectedRoute({ children, role }) {
  const { token, role: userRole } = useAuth()

  if (!token) return <Navigate to="/login" replace />
  if (role && userRole !== role) return <Navigate to="/unauthorized" replace />

  return children
}