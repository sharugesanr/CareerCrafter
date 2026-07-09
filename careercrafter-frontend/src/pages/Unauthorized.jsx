import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function Unauthorized() {
  const navigate = useNavigate()
  const { token, role, logout } = useAuth()

  const handleGoHome = () => {
    if (role === 'JobSeeker') navigate('/jobseeker/jobs')
    else if (role === 'Employer') navigate('/employer/listings')
    else if (role === 'Admin') navigate('/admin/dashboard')
    else navigate('/login')
  }

  const handleLoginAsDifferentUser = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div className="text-center">
        <h1 className="display-1 fw-bold text-danger">403</h1>
        <h4 className="mb-3">Access Denied</h4>
        <p className="text-muted mb-4">
          You don't have permission to view this page with your current role.
        </p>
        <div className="d-flex gap-2 justify-content-center">
          {token && (
            <button className="btn btn-primary" onClick={handleGoHome}>
              Go to My Dashboard
            </button>
          )}
          <button className="btn btn-outline-secondary" onClick={handleLoginAsDifferentUser}>
            Login as Different User
          </button>
        </div>
      </div>
    </div>
  )
}