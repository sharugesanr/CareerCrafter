import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../services/api'

export default function PostJob() {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    title: '', description: '', location: '', jobType: '',
    salaryRange: '', requiredSkills: '', deadline: ''
  })
  const [errors, setErrors] = useState({})
  const [serverError, setServerError] = useState('')
  const [loading, setLoading] = useState(false)

  const validate = () => {
    const errs = {}
    if (!form.title.trim()) errs.title = 'Job title is required.'
    if (!form.description.trim()) errs.description = 'Description is required.'
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
      await api.post('/jobs', {
        ...form,
        deadline: form.deadline || null
      })
      navigate('/employer/listings')
    } catch (err) {
      setServerError(err.response?.data?.message || 'Failed to post job.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      <h4 className="mb-4">Post a New Job</h4>
      {serverError && <div className="alert alert-danger">{serverError}</div>}
      <div className="card p-4 shadow-sm">
        <form onSubmit={handleSubmit} noValidate>
          <div className="mb-3">
            <label className="form-label">Job Title <span className="text-danger">*</span></label>
            <input type="text" name="title" className={`form-control ${errors.title ? 'is-invalid' : ''}`}
              value={form.title} onChange={handleChange} placeholder="e.g. Software Engineer" />
            {errors.title && <div className="invalid-feedback">{errors.title}</div>}
          </div>
          <div className="mb-3">
            <label className="form-label">Description <span className="text-danger">*</span></label>
            <textarea name="description" rows={5}
              className={`form-control ${errors.description ? 'is-invalid' : ''}`}
              value={form.description} onChange={handleChange}
              placeholder="Describe the role, responsibilities, requirements..." />
            {errors.description && <div className="invalid-feedback">{errors.description}</div>}
          </div>
          <div className="row g-3 mb-3">
            <div className="col-md-6">
              <label className="form-label">Location</label>
              <input type="text" name="location" className="form-control"
                value={form.location} onChange={handleChange} placeholder="e.g. Chennai, Remote" />
            </div>
            <div className="col-md-6">
              <label className="form-label">Job Type</label>
              <select name="jobType" className="form-select" value={form.jobType} onChange={handleChange}>
                <option value="">Select type</option>
                <option value="Full-time">Full-time</option>
                <option value="Part-time">Part-time</option>
                <option value="Contract">Contract</option>
                <option value="Internship">Internship</option>
              </select>
            </div>
          </div>
          <div className="row g-3 mb-3">
            <div className="col-md-6">
              <label className="form-label">Salary Range</label>
              <select name="salaryRange" className="form-select" value={form.salaryRange} onChange={handleChange}>
                <option value="">Select range</option>
                <option value="0-3 LPA">0-3 LPA</option>
                <option value="3-6 LPA">3-6 LPA</option>
                <option value="6-10 LPA">6-10 LPA</option>
                <option value="10-15 LPA">10-15 LPA</option>
                <option value="15+ LPA">15+ LPA</option>
              </select>
            </div>
            <div className="col-md-6">
              <label className="form-label">Application Deadline</label>
              <input type="date" name="deadline" className="form-control"
                value={form.deadline} onChange={handleChange}
                min={new Date().toISOString().split('T')[0]} />
            </div>
          </div>
          <div className="mb-3">
            <label className="form-label">Required Skills <small className="text-muted">(comma separated)</small></label>
            <input type="text" name="requiredSkills" className="form-control"
              value={form.requiredSkills} onChange={handleChange}
              placeholder="e.g. React, .NET, SQL Server" />
          </div>
          <div className="d-flex gap-2">
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Posting...' : 'Post Job'}
            </button>
            <button type="button" className="btn btn-secondary"
              onClick={() => navigate('/employer/listings')}>Cancel</button>
          </div>
        </form>
      </div>
    </div>
  )
}