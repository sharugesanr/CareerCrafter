import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import api from '../../services/api'
import { useAuth } from '../../context/AuthContext'

export default function Register() {
  const navigate = useNavigate()
  const { login } = useAuth()

  const [form, setForm] = useState({
    fullName: '', email: '', password: '', confirmPassword: '', role: 'JobSeeker', companyName: ''
  })
  const [errors, setErrors] = useState({})
  const [serverError, setServerError] = useState('')
  const [loading, setLoading] = useState(false)

  const getPasswordStrength = (password) => {
    if (!password) return { label: '', color: '' }
    const strong = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$/.test(password)
    const medium = /^(?=.*[a-zA-Z])(?=.*\d).{6,}$/.test(password)
    if (strong) return { label: 'Strong', color: 'text-success' }
    if (medium) return { label: 'Medium', color: 'text-warning' }
    return { label: 'Weak', color: 'text-danger' }
  }

  const validate = () => {
    const errs = {}
    if (!form.fullName.trim()) errs.fullName = 'Full name is required.'
    if (!form.email) errs.email = 'Email is required.'
    else if (!/\S+@\S+\.\S+/.test(form.email)) errs.email = 'Enter a valid email.'
    if (!form.password) errs.password = 'Password is required.'
    else if (form.password.length < 8) errs.password = 'Password must be at least 8 characters.'
    else if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$/.test(form.password))
      errs.password = 'Must include uppercase, lowercase, number and special character.'
    if (form.password !== form.confirmPassword) errs.confirmPassword = 'Passwords do not match.'
    if (form.role === 'Employer' && !form.companyName.trim()) errs.companyName = 'Company name is required.'
    return errs
  }

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value })
    setErrors({ ...errors, [e.target.name]: '' })
    setServerError('')
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    const errs = validate()
    if (Object.keys(errs).length > 0) { setErrors(errs); return }

    setLoading(true)
    try {
      const res=await api.post('/auth/register', {
        fullName: form.fullName,
        email: form.email,
        password: form.password,
        role: form.role,
        companyName: form.role === 'Employer' ? form.companyName : null
      })
    
      login(res.data)
      if (res.data.role === 'JobSeeker') navigate('/jobseeker/jobs')
      else if (res.data.role === 'Employer') navigate('/employer/listings')
    } catch (err) {
      setServerError(err.response?.data?.message || 'Registration failed. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  const strength = getPasswordStrength(form.password)

  return (
    <>
      <h5 className="text-center mb-3">Create an account</h5>
      {serverError && <div className="alert alert-danger">{serverError}</div>}
      <form onSubmit={handleSubmit} noValidate>
        <div className="mb-3">
          <label className="form-label">Full Name</label>
          <input
            type="text" name="fullName"
            className={`form-control ${errors.fullName ? 'is-invalid' : ''}`}
            placeholder="Enter your full name"
            value={form.fullName} onChange={handleChange}
          />
          {errors.fullName && <div className="invalid-feedback">{errors.fullName}</div>}
        </div>
        <div className="mb-3">
          <label className="form-label">Email</label>
          <input
            type="email" name="email"
            className={`form-control ${errors.email ? 'is-invalid' : ''}`}
            placeholder="Enter your email"
            value={form.email} onChange={handleChange}
          />
          {errors.email && <div className="invalid-feedback">{errors.email}</div>}
        </div>
        <div className="mb-3">
          <label className="form-label">Password</label>
          <input
            type="password" name="password"
            className={`form-control ${errors.password ? 'is-invalid' : ''}`}
            placeholder="Enter your password"
            value={form.password} onChange={handleChange}
          />
          {form.password && (
            <small className={`${strength.color} fw-semibold`}>
              Password strength: {strength.label}
            </small>
          )}
          {errors.password && <div className="invalid-feedback d-block">{errors.password}</div>}
        </div>
        <div className="mb-3">
          <label className="form-label">Confirm Password</label>
          <input
            type="password" name="confirmPassword"
            className={`form-control ${errors.confirmPassword ? 'is-invalid' : ''}`}
            placeholder="Re-enter your password"
            value={form.confirmPassword} onChange={handleChange}
          />
          {errors.confirmPassword && <div className="invalid-feedback">{errors.confirmPassword}</div>}
        </div>
        <div className="mb-3">
          <label className="form-label">Register as</label>
          <select name="role" className="form-select" value={form.role} onChange={handleChange}>
            <option value="JobSeeker">Job Seeker</option>
            <option value="Employer">Employer</option>
          </select>
        </div>
        {form.role === 'Employer' && (
          <div className="mb-3">
            <label className="form-label">Company Name</label>
            <input
              type="text" name="companyName"
              className={`form-control ${errors.companyName ? 'is-invalid' : ''}`}
              placeholder="Enter your company name"
              value={form.companyName} onChange={handleChange}
            />
            {errors.companyName && <div className="invalid-feedback">{errors.companyName}</div>}
          </div>
        )}
        <button type="submit" className="btn btn-primary w-100" disabled={loading}>
          {loading ? 'Registering...' : 'Register'}
        </button>
      </form>
      <p className="text-center mt-3 mb-0">
        Already have an account? <Link to="/login">Login</Link>
      </p>
    </>
  )
}