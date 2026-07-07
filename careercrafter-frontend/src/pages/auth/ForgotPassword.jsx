import { FaEye, FaEyeSlash } from 'react-icons/fa'
import { useEffect, useState, useRef } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import api from '../../services/api'

export default function ForgotPassword() {
  const navigate = useNavigate()

  const [step, setStep] = useState(1)
  const [form, setForm] = useState({
    email: '',
    newPassword: '',
    confirmPassword: ''
  })
  
  // Separate state for the 6 OTP input characters
  const [otp, setOtp] = useState(Array(6).fill(''))
  const otpRefs = useRef([])

  const [errors, setErrors] = useState({})
  const [serverError, setServerError] = useState('')
  const [successMsg, setSuccessMsg] = useState('') // Native success message state
  const [flashMsg, setFlashMsg] = useState('') // For "OTP sent successfully" banner
  const [loading, setLoading] = useState(false)
  const [isResetLoading, setIsResetLoading] = useState(false);
  const [isResendLoading, setIsResendLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [countdown, setCountdown] = useState(60)
  const [canResend, setCanResend] = useState(false)

  // Timer logic for resending OTP
  useEffect(() => {
    if (step === 2 && countdown > 0) {
      setCanResend(false)
      const timer = setTimeout(() => {
        setCountdown(prev => prev - 1)
      }, 1000)
      return () => clearTimeout(timer)
    } else if (countdown === 0) {
      setCanResend(true)
    }
  }, [countdown, step])

  // Automatically clears flash notifications after 5 seconds
  useEffect(() => {
    if (flashMsg) {
      const flashTimer = setTimeout(() => {
        setFlashMsg('')
      }, 5000)
      return () => clearTimeout(flashTimer)
    }
  }, [flashMsg])

  const getPasswordStrength = (password) => {
    if (!password) return { label: '', color: '' }

    const strong = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$/.test(password)
    const medium = /^(?=.*[a-zA-Z])(?=.*\d).{6,}$/.test(password)

    if (strong) return { label: 'Strong', color: 'text-success' }
    if (medium) return { label: 'Medium', color: 'text-warning' }

    return { label: 'Weak', color: 'text-danger' }
  }

  const strength = getPasswordStrength(form.newPassword)

  const handleChange = (e) => {
    setForm({
      ...form,
      [e.target.name]: e.target.value
    })

    setErrors({
      ...errors,
      [e.target.name]: ''
    })

    setServerError('')
  }

  // Handle individual OTP character changes
  const handleOtpChange = (element, index) => {
    const value = element.value.replace(/[^0-9]/g, '') // numbers only
    if (!value) {
      const newOtp = [...otp]
      newOtp[index] = ''
      setOtp(newOtp)
      return
    }

    const newOtp = [...otp]
    // Capture only the last character entered if a user types over an existing digit
    newOtp[index] = value.substring(value.length - 1)
    setOtp(newOtp)
    setErrors(prev => ({ ...prev, otpCode: '' }))

    // Move focus down to next box automatically
    if (index < 5 && element.value) {
      otpRefs.current[index + 1].focus()
    }
  }

  // Handle backspacing between individual inputs cleanly
  const handleOtpKeyDown = (e, index) => {
    if (e.key === 'Backspace' && !otp[index] && index > 0) {
      otpRefs.current[index - 1].focus()
    }
  }

  const validateEmail = () => {
    const errs = {}
    if (!form.email) errs.email = 'Email is required.'
    else if (!/\S+@\S+\.\S+/.test(form.email)) errs.email = 'Enter a valid email.'
    return errs
  }

  const validateReset = () => {
    const errs = {}
    const otpString = otp.join('')

    if (otpString.length !== 6) {
      errs.otpCode = 'Please enter a complete 6-digit OTP.'
    }

    if (!form.newPassword) {
      errs.newPassword = 'Password is required.'
    } else if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$/.test(form.newPassword)) {
      errs.newPassword = 'Must include uppercase, lowercase, number and special character.'
    }

    if (form.confirmPassword !== form.newPassword) {
      errs.confirmPassword = 'Passwords do not match.'
    }

    return errs
  }

  const sendOtp = async (e) => {
    e.preventDefault()
    const errs = validateEmail()

    if (Object.keys(errs).length > 0) {
      setErrors(errs)
      return
    }

    setLoading(true)
    setServerError('')

    try {
      await api.post('/auth/forgot-password', {
        email: form.email
      })

      setFlashMsg(`OTP sent successfully to ${form.email}`)
      setCountdown(60)
      setStep(2)
    } catch (err) {
      setServerError(err.response?.data?.message || 'Unable to send OTP.')
    } finally {
      setLoading(false)
    }
  }

  const handleResendOtp = async () => {
    if (!canResend) return
    setIsResendLoading(true)
    setServerError('')

    try {
      await api.post('/auth/forgot-password', {
        email: form.email // Fixed: now contextually tracks the existing state string safely
      })

      setFlashMsg('A new OTP has been sent successfully to your email.')
      setCountdown(60)
      setCanResend(false)
      setOtp(Array(6).fill('')) // Clear fields on fresh retry
      if(otpRefs.current[0]) otpRefs.current[0].focus()
    } catch (err) {
      setServerError(err.response?.data?.message || 'Unable to resend OTP.')
    } finally {
      setIsResendLoading(false)
    }
  }

  const resetPassword = async (e) => {
    e.preventDefault()
    const errs = validateReset()

    if (Object.keys(errs).length > 0) {
      setErrors(errs)
      return
    }

    setIsResetLoading(true)
    setServerError('')

    try {
      await api.post('/auth/reset-password', {
        email: form.email, // Silently delivered from step 1 state
        otpCode: otp.join(''),
        newPassword: form.newPassword
      })

      setSuccessMsg('Password has been reset successfully! Redirecting you back to login...')
      
      // Delay navigation so the user can actually read the inline validation notification
      setTimeout(() => {
        navigate('/login')
      }, 3000)

    } catch (err) {
      setServerError(err.response?.data?.message || 'Password reset failed.')
    } finally {
      setIsResetLoading(false)
    }
  }

  return (
    <>
      <h5 className="text-center mb-4 fw-bold text-dark">
        Forgot Password
      </h5>

      {/* Global Errors */}
      {serverError && (
        <div className="alert alert-danger shadow-sm py-2 px-3 small">
          {serverError}
        </div>
      )}

      {/* Success Banner (Page Level redirection notification) */}
      {successMsg && (
        <div className="alert alert-success shadow-sm py-2 px-3 small">
          {successMsg}
        </div>
      )}

      {/* Disappearing Status Banner */}
      {flashMsg && (
        <div className="alert alert-info shadow-sm py-2 px-3 small border-0 bg-light-subtle text-primary fw-medium text-center">
          {flashMsg}
        </div>
      )}

      {step === 1 && !successMsg && (
        <form onSubmit={sendOtp} noValidate>
          <div className="mb-4">
            <label className="form-label text-secondary fw-semibold small">
              Email Address
            </label>
            <input
              type="email"
              name="email"
              className={`form-control form-control-lg ${errors.email ? 'is-invalid' : ''}`}
              placeholder="Enter your registered email"
              value={form.email}
              onChange={handleChange}
            />
            {errors.email && (
              <div className="invalid-feedback">
                {errors.email}
              </div>
            )}
          </div>

          <button
            type="submit"
            className="btn btn-primary btn-lg w-100 fw-semibold"
            disabled={loading}
          >
            {loading ? 'Sending OTP...' : 'Send OTP'}
          </button>
        </form>
      )}

      {step === 2 && !successMsg && (
        <form onSubmit={resetPassword} noValidate>
          
          {/* Linked 6-Digit Code Block Inputs */}
          <div className="mb-4 text-center">
            <label className="form-label d-block text-start text-secondary fw-semibold small mb-2">
              Verification Code
            </label>
            <div className="d-flex justify-content-between gap-2">
              {otp.map((data, index) => (
                <input
                  key={index}
                  type="text"
                  maxLength="1"
                  className={`form-control text-center fw-bold fs-4 p-0 ${errors.otpCode ? 'is-invalid border-danger' : ''}`}
                  style={{ width: '45px', height: '50px'}}
                  value={data}
                  ref={el => otpRefs.current[index] = el}
                  onChange={e => handleOtpChange(e.target, index)}
                  onKeyDown={e => handleOtpKeyDown(e, index)}
                />
              ))}
            </div>
            {errors.otpCode && (
              <div className="text-danger small text-start mt-2">
                {errors.otpCode}
              </div>
            )}
          </div>

          {/* New Password Input Group */}
          <div className="mb-3">
            <label className="form-label text-secondary fw-semibold small">
              New Password
            </label>
            <div className="input-group">
              <input
                type={showPassword ? 'text' : 'password'}
                name="newPassword"
                className={`form-control ${errors.newPassword ? 'is-invalid' : ''}`}
                placeholder="Enter new password"
                value={form.newPassword}
                onChange={handleChange}
                style={{ borderRight: 'none' }}
              />
              <span 
                className={`input-group-text bg-white border-start-0 ${errors.newPassword ? 'border-danger' : ''}`}
                style={{ cursor: 'pointer', color: '#6c757d' }}
                onClick={() => setShowPassword(!showPassword)}
              >
                {showPassword ? <FaEyeSlash /> : <FaEye />}
              </span>
            </div>

            {form.newPassword && (
              <div className="mt-1">
                <small className={`${strength.color} fw-semibold small`}>
                  Password strength: {strength.label}
                </small>
              </div>
            )}

            {errors.newPassword && (
              <div className="text-danger small mt-1">
                {errors.newPassword}
              </div>
            )}
          </div>

          {/* Confirm Password Input Group */}
          <div className="mb-4">
            <label className="form-label text-secondary fw-semibold small">
              Confirm New Password
            </label>
            <div className="input-group">
              <input
                type={showConfirmPassword ? 'text' : 'password'}
                name="confirmPassword"
                className={`form-control ${errors.confirmPassword ? 'is-invalid' : ''}`}
                placeholder="Confirm password"
                value={form.confirmPassword}
                onChange={handleChange}
                style={{ borderRight: 'none' }}
              />
              <span 
                className={`input-group-text bg-white border-start-0 ${errors.confirmPassword ? 'border-danger' : ''}`}
                style={{ cursor: 'pointer', color: '#6c757d' }}
                onClick={() => setShowConfirmPassword(!showConfirmPassword)} // Fixed target binding toggle issue here
              >
                {showConfirmPassword ? <FaEyeSlash /> : <FaEye />}
              </span>
            </div>

            {errors.confirmPassword && (
              <div className="text-danger small mt-1">
                {errors.confirmPassword}
              </div>
            )}
          </div>

          {/* Primary Operations Actions Layout */}
          <div className="d-flex flex-column gap-2">
            <button
              type="submit"
              className="btn btn-success btn-lg w-100 fw-semibold"
              disabled={isResetLoading || isResendLoading}
            >
              {isResetLoading ? 'Resetting Password...' : 'Reset Password'}
            </button>
            
            <button
            type="button"
            className="btn btn-outline-primary w-100 fw-semibold mt-2"
            disabled={!canResend || isResendLoading || isResetLoading}
            onClick={handleResendOtp}>
            {isResendLoading 
              ? 'Sending...' 
              : canResend 
                ? "Resend OTP" 
                : `Resend OTP in ${countdown}s`
            }
          </button>
          </div>
        </form>
      )}

      <p className="text-center mt-4 mb-0 small">
        <Link to="/login" className="text-decoration-none text-muted fw-medium">
          ← Back to Login
        </Link>
      </p>
    </>
  )
}