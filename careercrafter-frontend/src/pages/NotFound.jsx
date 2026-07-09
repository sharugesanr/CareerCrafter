import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function NotFound() {
  const navigate = useNavigate()
  const { token, role } = useAuth()

  const handleGoHome = () => {
    if (!token) { navigate('/login'); return }
    if (role === 'JobSeeker') navigate('/jobseeker/jobs')
    else if (role === 'Employer') navigate('/employer/listings')
    else if (role === 'Admin') navigate('/admin/dashboard')
    else navigate('/login')
  }

  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div className="text-center">
        <h1 className="display-1 fw-bold text-primary">404</h1>
        <h4 className="mb-3">Page Not Found</h4>
        <p className="text-muted mb-4">
          The page you're looking for doesn't exist or may have been moved.
        </p>
        <button className="btn btn-primary" onClick={handleGoHome}>
          {token ? 'Go to Dashboard' : 'Go to Login'}
        </button>
      </div>
    </div>
  )
}