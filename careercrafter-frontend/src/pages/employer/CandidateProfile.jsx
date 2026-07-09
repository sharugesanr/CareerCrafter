import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import api from '../../services/api'

export default function CandidateProfile() {
  const { applicationId } = useParams()
  const navigate = useNavigate()
  const [profile, setProfile] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    api.get(`/applications/${applicationId}/candidate-profile`)
      .then(res => setProfile(res.data))
      .catch(err => setError(err.response?.data?.message || 'Failed to load profile.'))
      .finally(() => setLoading(false))
  }, [applicationId])

  const handleDownloadResume = async () => {
    try {
      const res = await api.get(`/resume/${profile.resumeId}/download`, { responseType: 'blob' })
      const url = window.URL.createObjectURL(new Blob([res.data]))
      const link = document.createElement('a')
      link.href = url
      link.setAttribute('download', profile.resumeFileName)
      document.body.appendChild(link)
      link.click()
      link.remove()
    } catch (err) {
      console.error(err)
    }
  }

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary" /></div>
  if (error) return <div className="alert alert-danger mt-4">{error}</div>
  if (!profile) return null

  return (
    <div>
      <button className="btn btn-link ps-0 mb-3" onClick={() => navigate(-1)}>← Back to Applicants</button>
      <h4 className="mb-4">Candidate Profile</h4>

      <div className="row g-4">
        <div className="col-md-8">
          <div className="card p-4 shadow-sm mb-4">
            <h6 className="mb-3">Personal Information</h6>
            <p><strong>Name:</strong> {profile.fullName}</p>
            <p><strong>Email:</strong> {profile.email}</p>
            <p><strong>Phone:</strong> {profile.phoneNumber || 'Not provided'}</p>
            <p><strong>Location:</strong> {profile.location || 'Not provided'}</p>
            <p className="mb-0"><strong>Summary:</strong> {profile.summary || 'Not provided'}</p>
          </div>

          <div className="card p-4 shadow-sm mb-4">
            <h6 className="mb-3">Skills</h6>
            {profile.skills ? (
              <div>
                {profile.skills.split(',').map((skill, i) => (
                  <span key={i} className="badge bg-light text-dark border me-2 mb-2">
                    {skill.trim()}
                  </span>
                ))}
              </div>
            ) : (
              <p className="text-muted mb-0">No skills added.</p>
            )}
          </div>

          <div className="card p-4 shadow-sm mb-4">
            <h6 className="mb-3">Education</h6>
            {profile.educations.length === 0 ? (
              <p className="text-muted mb-0">No education added.</p>
            ) : (
              profile.educations.map((edu, i) => (
                <div key={i} className="mb-2 pb-2 border-bottom">
                  <p className="mb-0 fw-semibold">{edu.degree} — {edu.institution}</p>
                  <small className="text-muted">{edu.yearOfPassing}</small>
                </div>
              ))
            )}
          </div>

          <div className="card p-4 shadow-sm">
            <h6 className="mb-3">Experience</h6>
            {profile.experiences.length === 0 ? (
              <p className="text-muted mb-0">No experience added yet — candidate may be a fresher.</p>
            ) : (
              profile.experiences.map((exp, i) => (
                <div key={i} className="mb-2 pb-2 border-bottom">
                  <p className="mb-0 fw-semibold">{exp.jobTitle} at {exp.company}</p>
                  <small className="text-muted">{exp.duration}</small>
                  {exp.description && <p className="small mb-0 mt-1">{exp.description}</p>}
                </div>
              ))
            )}
          </div>
        </div>

        <div className="col-md-4">
          <div className="card p-3 shadow-sm">
            <h6 className="mb-3">Resume Submitted for This Application</h6>
            <p className="mb-1 fw-semibold">{profile.resumeFileName}</p>
            <small className="text-muted d-block mb-3">
              Uploaded: {profile.resumeUploadedAt ? new Date(profile.resumeUploadedAt).toLocaleDateString() : 'N/A'}
            </small>
            <button className="btn btn-primary w-100" onClick={handleDownloadResume}>
              Download Resume
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}