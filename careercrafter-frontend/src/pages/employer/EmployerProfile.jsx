import { useState, useEffect } from 'react'
import api from '../../services/api'

export default function EmployerProfile() {
  const [profile, setProfile] = useState(null)
  const [editing, setEditing] = useState(false)
  const [form, setForm] = useState({})
  const [message, setMessage] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.get('/employer/profile')
      .then(res => { setProfile(res.data); setForm(res.data) })
      .catch(err => console.error(err))
      .finally(() => setLoading(false))
  }, [])

  const handleUpdate = async (e) => {
    e.preventDefault()
    try {
      const res = await api.put('/employer/profile', {
        companyName: form.companyName,
        industry: form.industry,
        website: form.website,
        location: form.location,
        description: form.description
      })
      setProfile(res.data)
      setEditing(false)
      setMessage({ type: 'success', text: 'Profile updated successfully.' })
    } catch (err) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Update failed.' })
    }
  }

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary" /></div>

  return (
    <div>
      <h4 className="mb-4">Company Profile</h4>
      {message && <div className={`alert alert-${message.type}`}>{message.text}</div>}
      <div className="card p-4 shadow-sm">
        <div className="d-flex justify-content-between mb-3">
          <h6>Company Information</h6>
          {!editing && (
            <button className="btn btn-outline-primary btn-sm" onClick={() => setEditing(true)}>Edit</button>
          )}
        </div>
        {editing ? (
          <form onSubmit={handleUpdate}>
            <div className="mb-3">
              <label className="form-label">Company Name</label>
              <input type="text" className="form-control" value={form.companyName || ''}
                onChange={e => setForm({ ...form, companyName: e.target.value })} required />
            </div>
            <div className="mb-3">
              <label className="form-label">Industry</label>
              <input type="text" className="form-control" value={form.industry || ''}
                onChange={e => setForm({ ...form, industry: e.target.value })} />
            </div>
            <div className="mb-3">
              <label className="form-label">Website</label>
              <input type="url" className="form-control" value={form.website || ''}
                onChange={e => setForm({ ...form, website: e.target.value })} placeholder="https://..." />
            </div>
            <div className="mb-3">
              <label className="form-label">Location</label>
              <input type="text" className="form-control" value={form.location || ''}
                onChange={e => setForm({ ...form, location: e.target.value })} />
            </div>
            <div className="mb-3">
              <label className="form-label">Description</label>
              <textarea className="form-control" rows={4} value={form.description || ''}
                onChange={e => setForm({ ...form, description: e.target.value })} />
            </div>
            <div className="d-flex gap-2">
              <button type="submit" className="btn btn-primary">Save</button>
              <button type="button" className="btn btn-secondary"
                onClick={() => { setEditing(false); setForm(profile) }}>Cancel</button>
            </div>
          </form>
        ) : (
          <div>
            <p><strong>Company Name:</strong> {profile.companyName}</p>
            <p><strong>Industry:</strong> {profile.industry || 'Not set'}</p>
            <p><strong>Website:</strong> {profile.website
              ? <a href={profile.website} target="_blank" rel="noreferrer">{profile.website}</a>
              : 'Not set'}
            </p>
            <p><strong>Location:</strong> {profile.location || 'Not set'}</p>
            <p><strong>Description:</strong> {profile.description || 'Not set'}</p>
            <hr />
            <p><strong>Contact Name:</strong> {profile.fullName}</p>
            <p><strong>Contact Email:</strong> {profile.email}</p>
          </div>
        )}
      </div>
    </div>
  )
}