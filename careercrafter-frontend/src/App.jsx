import { Routes, Route, Navigate } from 'react-router-dom'
import AuthLayout from './layouts/AuthLayout'
import MainLayout from './layouts/MainLayout'
import ProtectedRoute from './components/ProtectedRoute'
import Login from './pages/auth/Login'
import Register from './pages/auth/Register'
import ForgotPassword from './pages/auth/ForgotPassword'
import JobSearch from './pages/jobseeker/JobSearch'
import JobDetail from './pages/jobseeker/JobDetail'

import MyApplications from './pages/jobseeker/MyApplications'
import Profile from './pages/jobseeker/Profile'
import RecommendedJobs from './pages/jobseeker/RecommendedJobs'
import Resumes from './pages/jobseeker/Resumes'

import EmployerProfile from './pages/employer/EmployerProfile'
import PostJob from './pages/employer/PostJob'
import MyListings from './pages/employer/MyListings'
import EditJob from './pages/employer/EditJob'
import Applicants from './pages/employer/Applicants'

import AdminDashboard from './pages/admin/AdminDashboard'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />

      {/* Auth pages */}
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/forgot-password" element={<ForgotPassword />} />
      </Route>

      {/* JobSeeker pages — add here later */}
      <Route element={<ProtectedRoute role="JobSeeker"><MainLayout /></ProtectedRoute>}>
        <Route path="/jobseeker/jobs" element={<JobSearch />} />
        <Route path="/jobseeker/jobs/:id" element={<JobDetail />} />
        <Route path="/jobseeker/recommended" element={<RecommendedJobs/>} />
        <Route path="/jobseeker/applications" element={<MyApplications />} />
        <Route path="/jobseeker/resumes" element={<Resumes/>} />
        <Route path="/jobseeker/profile" element={<Profile/>} />
      </Route>

      {/* Employer pages — add here later */}
      <Route element={<ProtectedRoute role="Employer"><MainLayout /></ProtectedRoute>}>
        <Route path="/employer/listings" element={<MyListings />} />
        <Route path="/employer/post-job" element={<PostJob />} />
        <Route path="/employer/edit-job/:id" element={<EditJob />} />
        <Route path="/employer/applicants/:jobId" element={<Applicants />} />
        <Route path="/employer/profile" element={<EmployerProfile />} />
      </Route>

      {/* Admin */}
      <Route element={<ProtectedRoute role="Admin"><MainLayout /></ProtectedRoute>}>
        <Route path="/admin/dashboard" element={<AdminDashboard />} />
      </Route>

      <Route path="/unauthorized" element={<div className="text-center mt-5"><h3>Access Denied</h3></div>} />
    </Routes>
  )
}