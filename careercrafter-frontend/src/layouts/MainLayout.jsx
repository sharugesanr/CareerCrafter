import { Outlet, NavLink,Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'


export default function MainLayout() {
  const { user, role, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <>
      <nav className="navbar navbar-expand-lg navbar-dark bg-primary px-4">
        <span className="navbar-brand fw-bold fs-3">CareerCrafter</span>
        <div className="collapse navbar-collapse">
          <ul className="navbar-nav me-auto">
            {role === 'JobSeeker' && (
              <>
                <li className="nav-item">
                  <NavLink className={({ isActive }) => `nav-link ${isActive ? 'active fw-bold text-white' : ''}`} to="/jobseeker/jobs">Search Jobs</NavLink>
                </li>
                <li className="nav-item">
                  <NavLink className={({ isActive }) => `nav-link ${isActive ? 'active fw-bold text-white' : ''}`} to="/jobseeker/recommended">Recommended</NavLink>
                </li>
                <li className="nav-item">
                  <NavLink className={({ isActive }) => `nav-link ${isActive ? 'active fw-bold text-white' : ''}`} to="/jobseeker/applications">My Applications</NavLink>
                </li>
                <li className="nav-item">
                  <NavLink className={({ isActive }) => `nav-link ${isActive ? 'active fw-bold text-white' : ''}`} to="/jobseeker/resumes">Resumes</NavLink>
                </li>
                <li className="nav-item">
                  <NavLink className={({ isActive }) => `nav-link ${isActive ? 'active fw-bold text-white' : ''}`} to="/jobseeker/profile">Profile</NavLink>
                </li>
              </>
            )}
            {role === 'Employer' && (
              <>
                <li className="nav-item">
                  <NavLink className={({ isActive }) => `nav-link ${isActive ? 'active fw-bold text-white' : ''}`} to="/employer/listings">My Listings</NavLink>
                </li>
                <li className="nav-item">
                  <NavLink className={({ isActive }) => `nav-link ${isActive ? 'active fw-bold text-white' : ''}`} to="/employer/post-job">Post Job</NavLink>
                </li>
                <li className="nav-item">
                  <NavLink className={({ isActive }) => `nav-link ${isActive ? 'active fw-bold text-white' : ''}`} to="/employer/profile">Profile</NavLink>
                </li>
              </>
            )}
            {role === 'Admin' && (
              <li className="nav-item">
                <NavLink className={({ isActive }) => `nav-link ${isActive ? 'active fw-bold text-white' : ''}`} to="/admin/dashboard">Dashboard</NavLink>
              </li>
            )}
          </ul>

          <ul className="navbar-nav ms-auto">
            <li className="nav-item">
              <span className="nav-link text-white-50">Hi, {user?.fullName}</span>
            </li>
            <li className="nav-item">
              <button className="btn btn-outline-light btn-sm" onClick={handleLogout}>Logout</button>
            </li>
          </ul>
        </div>
      </nav>
      <div className="container mt-4">
        <Outlet />
      </div>
    </>
  )
}