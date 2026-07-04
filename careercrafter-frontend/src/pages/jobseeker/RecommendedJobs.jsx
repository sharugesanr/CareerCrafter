import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../services/api'

export default function RecommendedJobs() {
  const navigate = useNavigate()
  const [jobs, setJobs] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.get('/jobs/recommended')
      .then(res => setJobs(res.data))
      .catch(err => console.error(err))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary" /></div>

  return (
    <div>
      <h4 className="mb-4">Recommended for You</h4>
      {jobs.length === 0 ? (
        <div className="text-center text-muted py-5">
          <p>No recommendations yet.</p>
          <small>Update your profile with skills to get job recommendations.</small>
        </div>
      ) : (
        jobs.map(job => (
          <div key={job.jobId} className="card mb-3 shadow-sm">
            <div className="card-body d-flex justify-content-between align-items-start">
              <div>
                <h5 className="mb-1">{job.title}</h5>
                <p className="text-muted mb-1">{job.companyName} — {job.location}</p>
                <span className="badge bg-secondary me-2">{job.jobType}</span>
                <span className="badge bg-light text-dark border">{job.salaryRange}</span>
              </div>
              <button className="btn btn-outline-primary btn-sm"
                onClick={() => navigate(`/jobseeker/jobs/${job.jobId}`)}>
                View Details
              </button>
            </div>
          </div>
        ))
      )}
    </div>
  )
}