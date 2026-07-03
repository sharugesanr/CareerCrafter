import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import api from '../../services/api'

export default function JobDetail() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [job, setJob] = useState(null)
  const [resumes, setResumes] = useState([])
  const [selectedResume, setSelectedResume] = useState('')
  const [coverNote, setCoverNote] = useState('')
  const [loading, setLoading] = useState(true)
  const [applying, setApplying] = useState(false)
  const [message, setMessage] = useState(null)

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [jobRes, resumeRes] = await Promise.all([
          api.get(`/jobs/${id}`),
          api.get('/resume')
        ])
        setJob(jobRes.data)
        setResumes(resumeRes.data)
      } catch (err) {
        console.error(err)
      } finally {
        setLoading(false)
      }
    }
    fetchData()
  }, [id])

  const handleApply = async () => {
    if (!selectedResume) { setMessage({ type: 'danger', text: 'Please select a resume.' }); return }
    setApplying(true)
    try {
      await api.post('/applications', {
        jobId: parseInt(id),
        resumeId: parseInt(selectedResume),
        coverNote
      })
      setMessage({ type: 'success', text: 'Application submitted successfully!' })
    } catch (err) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Failed to apply.' })
    } finally {
      setApplying(false)
    }
  }

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary" /></div>
  if (!job) return <div className="text-center text-muted py-5">Job not found.</div>

  return (
    <div className="row">
      <div className="col-md-8">
        <button className="btn btn-link ps-0 mb-3" onClick={() => navigate(-1)}>← Back</button>
        <h4>{job.title}</h4>
        <p className="text-muted">{job.companyName} · {job.location}</p>
        <div className="mb-2">
          <span className="badge bg-secondary me-2">{job.jobType}</span>
          <span className="badge bg-light text-dark border">{job.salaryRange}</span>
        </div>
        <hr />
        <h6>Description</h6>
        <p style={{ whiteSpace: 'pre-line' }}>{job.description}</p>
        <h6>Required Skills</h6>
        <p>{job.requiredSkills || 'Not specified'}</p>
        {job.deadline && <p className="text-muted small">Deadline: {new Date(job.deadline).toLocaleDateString()}</p>}
      </div>

      <div className="col-md-4">
        <div className="card p-3 shadow-sm">
          <h6 className="mb-3">Apply for this job</h6>
          {message && <div className={`alert alert-${message.type}`}>{message.text}</div>}
          <div className="mb-3">
            <label className="form-label">Select Resume</label>
            <select className="form-select" value={selectedResume}
              onChange={e => setSelectedResume(e.target.value)}>
              <option value="">-- Choose resume --</option>
              {resumes.map(r => (
                <option key={r.resumeId} value={r.resumeId}>{r.fileName}</option>
              ))}
            </select>
            {resumes.length === 0 && (
              <small className="text-danger">No resumes uploaded. <a href="/jobseeker/resumes">Upload one</a></small>
            )}
          </div>
          <div className="mb-3">
            <label className="form-label">Cover Note (optional)</label>
            <textarea className="form-control" rows={3} value={coverNote}
              onChange={e => setCoverNote(e.target.value)} placeholder="Write a short note..." />
          </div>
          <button className="btn btn-primary w-100" onClick={handleApply} disabled={applying}>
            {applying ? 'Applying...' : 'Apply Now'}
          </button>
        </div>
      </div>
    </div>
  )
}