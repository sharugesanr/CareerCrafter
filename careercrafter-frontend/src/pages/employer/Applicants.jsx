import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import api from '../../services/api'

const statusColors = {
  Applied: 'primary', Reviewed: 'info',
  Shortlisted: 'success', Rejected: 'danger', Withdrawn: 'secondary'
}

export default function Applicants() {
  const { jobId } = useParams()
  const navigate = useNavigate()
  const [applicants, setApplicants] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState(null)

  useEffect(() => {
    api.get(`/applications/job/${jobId}`)
      .then(res => setApplicants(res.data))
      .catch(err => console.error(err))
      .finally(() => setLoading(false))
  }, [jobId])

  const handleStatusChange = async (applicationId, newStatus) => {
    try {
      await api.patch(`/applications/${applicationId}/status`, { status: newStatus })
      setApplicants(applicants.map(a =>
        a.applicationId === applicationId ? { ...a, status: newStatus } : a
      ))
      setMessage({ type: 'success', text: `Status updated to ${newStatus}.` })
    } catch (err) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Failed to update.' })
    }
  }

  const handleDownloadResume = async (resumeId, fileName) => {
  try {
    const res = await api.get(`/resume/${resumeId}/download`, { responseType: 'blob' })
    const url = window.URL.createObjectURL(new Blob([res.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', fileName)
    document.body.appendChild(link)
    link.click()
    link.remove()
  } catch (err) {
    setMessage({ type: 'danger', text: 'Failed to download resume.' })
  }
}

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary" /></div>

  return (
    <div>
      <button className="btn btn-link ps-0 mb-3" onClick={() => navigate('/employer/listings')}>
        ← Back to Listings
      </button>
      <h4 className="mb-4">Applicants</h4>
      {message && <div className={`alert alert-${message.type}`}>{message.text}</div>}
      {applicants.length === 0 ? (
        <div className="text-center text-muted py-5">No applicants yet for this job.</div>
      ) : (
        applicants.map(app => (
          <div key={app.applicationId} className="card mb-3 shadow-sm">
            <div className="card-body">
              <div className="d-flex justify-content-between align-items-start">
                <div>
                  <h6 className="mb-1">{app.jobSeekerName}</h6>
                  <p className="text-muted small mb-1">{app.jobSeekerEmail}</p>
                  <span className={`badge bg-${statusColors[app.status] || 'secondary'} mb-2`}>
                    {app.status}
                  </span>
                  {app.coverNote && <p className="small mb-1">Note: {app.coverNote}</p>}
                  <p className="text-muted small mb-0">
                    Applied: {new Date(app.appliedAt).toLocaleDateString()}
                  </p>
                </div>
                <div className="d-flex flex-column gap-2 align-items-end">
                  {app.status !== 'Withdrawn' && (
                    <>
                      <button className="btn btn-outline-primary btn-sm"
                        onClick={() => navigate(`/employer/candidate-profile/${app.applicationId}`)}>
                        View Profile
                      </button>
                      <button className="btn btn-outline-secondary btn-sm"
                        onClick={() => handleDownloadResume(app.resumeId, app.resumeFileName)}>
                        Download Resume
                      </button>
                    </>
                  )}
                  {app.status !== 'Withdrawn' && app.status !== 'Rejected' && (
                    <select className="form-select form-select-sm"
                      value={app.status}
                      onChange={e => handleStatusChange(app.applicationId, e.target.value)}>
                      <option value="Applied">Applied</option>
                      <option value="Reviewed">Reviewed</option>
                      <option value="Shortlisted">Shortlisted</option>
                      <option value="Rejected">Rejected</option>
                    </select>
                  )}
                </div>
              </div>
            </div>
          </div>
        ))
      )}
    </div>
  )
}