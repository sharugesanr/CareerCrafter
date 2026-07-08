import { useState, useEffect } from 'react'
import api from '../../services/api'

export default function Profile() {
  const [profile, setProfile] = useState(null)
  const [educations, setEducations] = useState([])
  const [experiences, setExperiences] = useState([])
  const [activeTab, setActiveTab] = useState('profile')
  const [editing, setEditing] = useState(false)
  const [form, setForm] = useState({})
  const [message, setMessage] = useState(null)
  const [loading, setLoading] = useState(true)

  // Education form
  const [eduForm, setEduForm] = useState({ degree: '', institution: '', yearOfPassing: '' })
  const [expForm, setExpForm] = useState({ jobTitle: '', company: '', duration: '', description: '' })

  useEffect(() => {
    const fetchAll = async () => {
      try {
        const [profileRes, eduRes, expRes] = await Promise.all([
          api.get('/jobseeker/profile'),
          api.get('/jobseeker/education'),
          api.get('/jobseeker/experience')
        ])
        setProfile(profileRes.data)
        setForm(profileRes.data)
        setEducations(eduRes.data)
        setExperiences(expRes.data)
      } catch (err) {
        console.error(err)
      } finally {
        setLoading(false)
      }
    }
    fetchAll()
  }, [])

  const handleProfileUpdate = async (e) => {
    e.preventDefault()
    try {
      const res = await api.put('/jobseeker/profile', {
        phoneNumber: form.phoneNumber,
        location: form.location,
        summary: form.summary,
        skills: form.skills
      })
      setProfile(res.data)
      setEditing(false)
      setMessage({ type: 'success', text: 'Profile updated successfully.' })
    } catch (err) {
      // 1. Fallback default message
    let errorText = 'Update failed.' 

    // 2. Check if the server returned an explicit validation errors object
    const backendErrors = err.response?.data?.errors 

    if (backendErrors) {
      // Extract all nested validation error messages and join them together
      errorText = Object.values(backendErrors)
        .flatMap(errorArray => errorArray)
        .join(' ')
    } else if (err.response?.data?.message) {
      // 3. Fallback check for alternative standard error fields
      errorText = err.response.data.message
    }

    setMessage({ type: 'danger', text: errorText })
    }
  }

  const handleAddEducation = async (e) => {
    e.preventDefault()
    try {
      const res = await api.post('/jobseeker/education', eduForm)
      setEducations([...educations, res.data])
      setEduForm({ degree: '', institution: '', yearOfPassing: '' })
      setMessage({ type: 'success', text: 'Education added.' })
    } catch (err) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Failed to add.' })
    }
  }

  const handleDeleteEducation = async (id) => {
    if (!window.confirm('Delete this education record?')) return
    try {
      await api.delete(`/jobseeker/education/${id}`)
      setEducations(educations.filter(e => e.educationId !== id))
    } catch (err) {
      setMessage({ type: 'danger', text: 'Failed to delete.' })
    }
  }

  const handleAddExperience = async (e) => {
    e.preventDefault()
    try {
      const res = await api.post('/jobseeker/experience', expForm)
      setExperiences([...experiences, res.data])
      setExpForm({ jobTitle: '', company: '', duration: '', description: '' })
      setMessage({ type: 'success', text: 'Experience added.' })
    } catch (err) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Failed to add.' })
    }
  }

  const handleDeleteExperience = async (id) => {
    if (!window.confirm('Delete this experience record?')) return
    try {
      await api.delete(`/jobseeker/experience/${id}`)
      setExperiences(experiences.filter(e => e.experienceId !== id))
    } catch (err) {
      setMessage({ type: 'danger', text: 'Failed to delete.' })
    }
  }

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary" /></div>

  return (
    <div>
      <h4 className="mb-4">My Profile</h4>
      {message && <div className={`alert alert-${message.type}`}>{message.text}</div>}

      {/* Tabs */}
      <ul className="nav nav-tabs mb-4">
        {['profile', 'education', 'experience'].map(tab => (
          <li key={tab} className="nav-item">
            <button className={`nav-link ${activeTab === tab ? 'active' : ''}`}
              onClick={() => setActiveTab(tab)}>
              {tab.charAt(0).toUpperCase() + tab.slice(1)}
            </button>
          </li>
        ))}
      </ul>

      {/* Profile Tab */}
      {activeTab === 'profile' && (
        <div className="card p-4 shadow-sm">
          <div className="d-flex justify-content-between mb-3">
            <h6>Personal Information</h6>
            {!editing && (
              <button className="btn btn-outline-primary btn-sm" onClick={() => setEditing(true)}>Edit</button>
            )}
          </div>
          {editing ? (
            <form onSubmit={handleProfileUpdate}>
              <div className="mb-3">
                <label className="form-label">Phone Number</label>
                <input type="text" className="form-control" value={form.phoneNumber || ''}
                  onChange={e => setForm({ ...form, phoneNumber: e.target.value })} />
              </div>
              <div className="mb-3">
                <label className="form-label">Location</label>
                <input type="text" className="form-control" value={form.location || ''}
                  onChange={e => setForm({ ...form, location: e.target.value })} />
              </div>
              <div className="mb-3">
                <label className="form-label">Summary</label>
                <textarea className="form-control" rows={3} value={form.summary || ''}
                  onChange={e => setForm({ ...form, summary: e.target.value })} />
              </div>
              <div className="mb-3">
                <label className="form-label">Skills <small className="text-muted">(comma separated)</small></label>
                <input type="text" className="form-control" value={form.skills || ''}
                  onChange={e => setForm({ ...form, skills: e.target.value })}
                  placeholder="e.g. React, .NET, SQL" />
              </div>
              <div className="d-flex gap-2">
                <button type="submit" className="btn btn-primary">Save</button>
                <button type="button" className="btn btn-secondary"
                  onClick={() => { setEditing(false); setForm(profile) }}>Cancel</button>
              </div>
            </form>
          ) : (
            <div>
              <p><strong>Name:</strong> {profile.fullName}</p>
              <p><strong>Email:</strong> {profile.email}</p>
              <p><strong>Phone:</strong> {profile.phoneNumber || 'Not set'}</p>
              <p><strong>Location:</strong> {profile.location || 'Not set'}</p>
              <p><strong>Summary:</strong> {profile.summary || 'Not set'}</p>
              <p><strong>Skills:</strong> {profile.skills || 'Not set'}</p>
            </div>
          )}
        </div>
      )}

      {/* Education Tab */}
      {activeTab === 'education' && (
        <div>
          <div className="card p-3 mb-4 shadow-sm">
            <h6 className="mb-3">Add Education</h6>
            <form onSubmit={handleAddEducation}>
              <div className="row g-2">
                <div className="col-md-4">
                  <input type="text" className="form-control" placeholder="Degree"
                    value={eduForm.degree} onChange={e => setEduForm({ ...eduForm, degree: e.target.value })} required />
                </div>
                <div className="col-md-4">
                  <input type="text" className="form-control" placeholder="Institution"
                    value={eduForm.institution} onChange={e => setEduForm({ ...eduForm, institution: e.target.value })} required />
                </div>
                <div className="col-md-2">
                  <input type="number" className="form-control" placeholder="Year"
                    value={eduForm.yearOfPassing} onChange={e => setEduForm({ ...eduForm, yearOfPassing: e.target.value })} />
                </div>
                <div className="col-md-2">
                  <button type="submit" className="btn btn-primary w-100">Add</button>
                </div>
              </div>
            </form>
          </div>
          {educations.length === 0 ? (
            <p className="text-muted">No education records added yet.</p>
          ) : (
            educations.map(edu => (
              <div key={edu.educationId} className="card mb-2 shadow-sm">
                <div className="card-body d-flex justify-content-between align-items-center">
                  <div>
                    <p className="mb-0 fw-semibold">{edu.degree} — {edu.institution}</p>
                    <small className="text-muted">{edu.yearOfPassing}</small>
                  </div>
                  <button className="btn btn-outline-danger btn-sm"
                    onClick={() => handleDeleteEducation(edu.educationId)}>Delete</button>
                </div>
              </div>
            ))
          )}
        </div>
      )}

      {/* Experience Tab */}
      {activeTab === 'experience' && (
        <div>
          <div className="card p-3 mb-4 shadow-sm">
            <h6 className="mb-3">Add Experience</h6>
            <form onSubmit={handleAddExperience}>
              <div className="row g-2 mb-2">
                <div className="col-md-4">
                  <input type="text" className="form-control" placeholder="Job Title"
                    value={expForm.jobTitle} onChange={e => setExpForm({ ...expForm, jobTitle: e.target.value })} required />
                </div>
                <div className="col-md-4">
                  <input type="text" className="form-control" placeholder="Company"
                    value={expForm.company} onChange={e => setExpForm({ ...expForm, company: e.target.value })} required />
                </div>
                <div className="col-md-4">
                  <input type="text" className="form-control" placeholder="Duration (e.g. 2 years)"
                    value={expForm.duration} onChange={e => setExpForm({ ...expForm, duration: e.target.value })} />
                </div>
              </div>
              <div className="row g-2">
                <div className="col-md-10">
                  <input type="text" className="form-control" placeholder="Description"
                    value={expForm.description} onChange={e => setExpForm({ ...expForm, description: e.target.value })} />
                </div>
                <div className="col-md-2">
                  <button type="submit" className="btn btn-primary w-100">Add</button>
                </div>
              </div>
            </form>
          </div>
          {experiences.length === 0 ? (
            <p className="text-muted">No experience records added yet.</p>
          ) : (
            experiences.map(exp => (
              <div key={exp.experienceId} className="card mb-2 shadow-sm">
                <div className="card-body d-flex justify-content-between align-items-center">
                  <div>
                    <p className="mb-0 fw-semibold">{exp.jobTitle} at {exp.company}</p>
                    <small className="text-muted">{exp.duration}</small>
                    {exp.description && <p className="small mb-0 mt-1">{exp.description}</p>}
                  </div>
                  <button className="btn btn-outline-danger btn-sm"
                    onClick={() => handleDeleteExperience(exp.experienceId)}>Delete</button>
                </div>
              </div>
            ))
          )}
        </div>
      )}
    </div>
  )
}