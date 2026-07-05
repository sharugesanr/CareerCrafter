import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import api from '../../services/api'

export default function EditJob() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [form, setForm] = useState(null)
  const [errors, setErrors] = useState({})
  const [serverError, setServerError] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    api.get(`/jobs/${id}`)
      .then(res => setForm({
        title: res.data.title,
        description: res.data.description,
        location: res.data.location || '',
        jobType: res.data.jobType || '',
        salaryRange: res.data.salaryRange || '',
        requiredSkills: res.data.requiredSkills || '',
        deadline: res.data.deadline ? res.data.deadline.split('T')[0] : ''
      }))
      .catch(err => console.error(err))
      .finally(() => setLoading(false))
  }, [id])

  const validate = () => {
    const errs = {}
    if (!form.title.trim()) errs.title = 'Job title is required.'
    if (!form.description.trim()) errs.description = 'Description is required.'
    return errs
  }

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value })
    setErrors({ ...errors, [e.target.name]: '' })
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    const errs = validate()
    if (Object.keys(errs).length > 0) { setErrors(errs); return }

    setSaving(true)
    try {
      await api.put(`/jobs/${id}`, { ...form, deadline: form.deadline || null })
      navigate('/employer/listings')
    } catch (err) {
      setServerError(err.response?.data?.message || 'Failed to update job.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary" /></div>

  return (
    <div>
      <h4 className="mb-4">Edit Job Listing</h4>
      {serverError && <div className="alert alert-danger">{serverError}</div>}
      <div className="card p-4 shadow-sm">
        <form onSubmit={handleSubmit} noValidate>
          <div className="mb-3">
            <label className="form-label">Job Title <span className="text-danger">*</span></label>
            <input type="text" name="title" className={`form-control ${errors.title ? 'is-invalid' : ''}`}
              value={form.title} onChange={handleChange} />
            {errors.title && <div className="invalid-feedback">{errors.title}</div>}
          </div>
          <div className="mb-3">
            <label className="form-label">Description <span className="text-danger">*</span></label>
            <textarea name="description" rows={5}
              className={`form-control ${errors.description ? 'is-invalid' : ''}`}
              value={form.description} onChange={handleChange} />
            {errors.description && <div className="invalid-feedback">{errors.description}</div>}
          </div>
          <div className="row g-3 mb-3">
            <div className="col-md-6">
              <label className="form-label">Location</label>
              <input type="text" name="location" className="form-control"
                value={form.location} onChange={handleChange} />
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
              <label className="form-label">Deadline</label>
              <input type="date" name="deadline" className="form-control"
                value={form.deadline} onChange={handleChange}
                min={new Date().toISOString().split('T')[0]} />
            </div>
          </div>
          <div className="mb-3">
            <label className="form-label">Required Skills</label>
            <input type="text" name="requiredSkills" className="form-control"
              value={form.requiredSkills} onChange={handleChange} />
          </div>
          <div className="d-flex gap-2">
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Saving...' : 'Save Changes'}
            </button>
            <button type="button" className="btn btn-secondary"
              onClick={() => navigate('/employer/listings')}>Cancel</button>
          </div>
        </form>
      </div>
    </div>
  )
}