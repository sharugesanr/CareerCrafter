import { useState, useEffect } from 'react'
import api from '../../services/api'

const statusColors = {
  Applied: 'primary',
  Reviewed: 'info',
  Shortlisted: 'success',
  Rejected: 'danger',
  Withdrawn: 'secondary'
}

export default function MyApplications() {
  const [applications, setApplications] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState(null)

  useEffect(() => {
    api.get('/applications/my-applications')
      .then(res => setApplications(res.data))
      .catch(err => console.error(err))
      .finally(() => setLoading(false))
  }, [])

  const handleWithdraw = async (applicationId) => {
    if (!window.confirm('Are you sure you want to withdraw this application?')) return
    try {
      await api.patch(`/applications/${applicationId}/withdraw`)
      setApplications(applications.map(a =>
        a.applicationId === applicationId ? { ...a, status: 'Withdrawn' } : a
      ))
      setMessage({ type: 'success', text: 'Application withdrawn.' })
    } catch (err) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Failed to withdraw.' })
    }
  }

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary" /></div>

  return (
    <div>
      <h4 className="mb-4">My Applications</h4>
      {message && <div className={`alert alert-${message.type}`}>{message.text}</div>}
      {applications.length === 0 ? (
        <div className="text-center text-muted py-5">You haven't applied to any jobs yet.</div>
      ) : (
        applications.map(app => (
          <div key={app.applicationId} className="card mb-3 shadow-sm">
            <div className="card-body d-flex justify-content-between align-items-start">
              <div>
                <h5 className="mb-1">{app.jobTitle}</h5>
                <p className="text-muted mb-1">{app.companyName}</p>
                <span className={`badge bg-${statusColors[app.status] || 'secondary'}`}>
                  {app.status}
                </span>
                <p className="text-muted small mt-2 mb-0">
                  Applied: {new Date(app.appliedAt).toLocaleDateString()}
                </p>
                {app.coverNote && <p className="small mt-1 mb-0">Note: {app.coverNote}</p>}
              </div>
              {app.status === 'Applied' && (
                <button className="btn btn-outline-danger btn-sm"
                  onClick={() => handleWithdraw(app.applicationId)}>
                  Withdraw
                </button>
              )}
            </div>
          </div>
        ))
      )}
    </div>
  )
}