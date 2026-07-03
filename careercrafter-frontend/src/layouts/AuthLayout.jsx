import { Outlet } from 'react-router-dom'

export default function AuthLayout() {
  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div className="card shadow p-4" style={{ width: '100%', maxWidth: '450px' }}>
        <div className="text-center mb-4">
          <h1 className="fw-bold text-primary">CareerCrafter</h1>
          <p className="text-muted">Your career journey starts here</p>
        </div>
        <Outlet />
      </div>
    </div>
  )
}