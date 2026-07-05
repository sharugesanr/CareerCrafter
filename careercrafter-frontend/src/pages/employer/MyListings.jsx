import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../services/api'

export default function MyListings() {
  const navigate = useNavigate()
  const [jobs, setJobs] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState(null)

  useEffect(() => {
    api.get('/jobs/my-listings')
      .then(res => setJobs(res.data))
      .catch(err => console.error(err))
      .finally(() => setLoading(false))
  }, [])

  const handleDeactivate = async (jobId) => {
    if (!window.confirm('Deactivate this job listing?')) return
    try {
      await api.delete(`/jobs/${jobId}`)
      setJobs(jobs.map(j => j.jobId === jobId ? { ...j, isActive: false } : j))
      setMessage({ type: 'success', text: 'Job deactivated.' })
    } catch (err) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Failed.' })
    }
  }

  const handleReactivate = async (jobId) => {
    try {
      await api.patch(`/jobs/${jobId}/reactivate`)
      setJobs(jobs.map(j => j.jobId === jobId ? { ...j, isActive: true } : j))
      setMessage({ type: 'success', text: 'Job reactivated.' })
    } catch (err) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Failed.' })
    }
  }

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary" /></div>

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h4 className="mb-0">My Job Listings</h4>
        <button className="btn btn-primary" onClick={() => navigate('/employer/post-job')}>
          + Post New Job
        </button>
      </div>
      {message && <div className={`alert alert-${message.type}`}>{message.text}</div>}
      {jobs.length === 0 ? (
        <div className="text-center text-muted py-5">No job listings yet.</div>
      ) : (
        jobs.map(job => (
          <div key={job.jobId} className="card mb-3 shadow-sm">
            <div className="card-body">
              <div className="d-flex justify-content-between align-items-start">
                <div>
                  <h5 className="mb-1">
                    {job.title}
                    <span className={`badge ms-2 ${job.isActive ? 'bg-success' : 'bg-secondary'}`}>
                      {job.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </h5>
                  <p className="text-muted mb-1">{job.location} · {job.jobType} · {job.salaryRange}</p>
                  <small className="text-muted">
                    Posted: {new Date(job.postedAt).toLocaleDateString()}
                    {job.deadline && ` · Deadline: ${new Date(job.deadline).toLocaleDateString()}`}
                  </small>
                </div>
                <div className="d-flex gap-2">
                  <button className="btn btn-outline-secondary btn-sm"
                    onClick={() => navigate(`/employer/applicants/${job.jobId}`)}>
                    View Applicants
                  </button>
                  <button className="btn btn-outline-primary btn-sm"
                    onClick={() => navigate(`/employer/edit-job/${job.jobId}`)}>
                    Edit
                  </button>
                  {job.isActive ? (
                    <button className="btn btn-outline-danger btn-sm"
                      onClick={() => handleDeactivate(job.jobId)}>Deactivate</button>
                  ) : (
                    <button className="btn btn-outline-success btn-sm"
                      onClick={() => handleReactivate(job.jobId)}>Reactivate</button>
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