import { useState, useEffect, useRef } from 'react'
import api from '../../services/api'

export default function Resumes() {
  const [resumes, setResumes] = useState([])
  const [loading, setLoading] = useState(true)
  const [uploading, setUploading] = useState(false)
  const [message, setMessage] = useState(null)
  const fileRef = useRef()

  useEffect(() => {
    fetchResumes()
  }, [])

  const fetchResumes = async () => {
    try {
      const res = await api.get('/resume')
      setResumes(res.data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const handleUpload = async () => {
    const file = fileRef.current.files[0]
    if (!file) { setMessage({ type: 'danger', text: 'Please select a file.' }); return }

    const formData = new FormData()
    formData.append('file', file)
    setUploading(true)
    try {
      await api.post('/resume/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      setMessage({ type: 'success', text: 'Resume uploaded successfully.' })
      fileRef.current.value = ''
      fetchResumes()
    } catch (err) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Upload failed.' })
    } finally {
      setUploading(false)
    }
  }

  const handleDelete = async (resumeId) => {
    if (!window.confirm('Delete this resume?')) return
    try {
      await api.delete(`/resume/${resumeId}`)
      setResumes(resumes.filter(r => r.resumeId !== resumeId))
      setMessage({ type: 'success', text: 'Resume deleted.' })
    } catch (err) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Delete failed.' })
    }
  }

  const handleDownload = async (resumeId, fileName) => {
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
      setMessage({ type: 'danger', text: 'Download failed.' })
    }
  }

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary" /></div>

  return (
    <div>
      <h4 className="mb-4">My Resumes</h4>
      {message && <div className={`alert alert-${message.type}`}>{message.text}</div>}

      <div className="card p-3 mb-4 shadow-sm">
        <h6>Upload New Resume</h6>
        <small className="text-muted d-block mb-2">PDF, DOC, DOCX only · Max 5MB · Up to 4 resumes</small>
        <div className="d-flex gap-2">
          <input type="file" className="form-control" ref={fileRef} accept=".pdf,.doc,.docx" />
          <button className="btn btn-primary" onClick={handleUpload} disabled={uploading}>
            {uploading ? 'Uploading...' : 'Upload'}
          </button>
        </div>
      </div>

      {resumes.length === 0 ? (
        <div className="text-center text-muted py-4">No resumes uploaded yet.</div>
      ) : (
        resumes.map(r => (
          <div key={r.resumeId} className="card mb-2 shadow-sm">
            <div className="card-body d-flex justify-content-between align-items-center">
              <div>
                <p className="mb-0 fw-semibold">{r.fileName}</p>
                <small className="text-muted">
                  Uploaded: {new Date(r.uploadedAt).toLocaleDateString()}
                </small>
              </div>
              <div className="d-flex gap-2">
                <button className="btn btn-outline-secondary btn-sm"
                  onClick={() => handleDownload(r.resumeId, r.fileName)}>
                  Download
                </button>
                <button className="btn btn-outline-danger btn-sm"
                  onClick={() => handleDelete(r.resumeId)}>
                  Delete
                </button>
              </div>
            </div>
          </div>
        ))
      )}
    </div>
  )
}