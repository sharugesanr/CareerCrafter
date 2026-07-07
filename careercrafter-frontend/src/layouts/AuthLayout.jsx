import { Outlet } from 'react-router-dom'
import logo from '../assets/logo.svg'

export default function AuthLayout() {
  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div className="card shadow p-4" style={{ width: '100%', maxWidth: '450px' }}>
        <div className="text-center mb-4">
          <img src={logo} alt="CareerCrafter Logo" width="60" height="60" className="mb-2" />
          <h2 className="fw-bold text-primary">CareerCrafter</h2>
          <p className="text-muted">Your career journey starts here</p>
        </div>
        <Outlet />
      </div>
    </div>
  )
}