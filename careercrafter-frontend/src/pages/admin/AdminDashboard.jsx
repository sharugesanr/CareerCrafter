import { useState, useEffect } from 'react'
import api from '../../services/api'

export default function AdminDashboard() {
  const [stats, setStats] = useState(null)
  const [users, setUsers] = useState([])
  const [jobs, setJobs] = useState([])
  const [activeTab, setActiveTab] = useState('users')
  const [loading, setLoading] = useState(true)
  const [purging, setPurging] = useState(false)
  const [message, setMessage] = useState(null)

  useEffect(() => {
    const fetchAll = async () => {
      try {
        const [statsRes, usersRes, jobsRes] = await Promise.all([
          api.get('/admin/stats'),
          api.get('/admin/users'),
          api.get('/admin/jobs')
        ])
        setStats(statsRes.data)
        setUsers(usersRes.data)
        setJobs(jobsRes.data)
      } catch (err) {
        console.error(err)
      } finally {
        setLoading(false)
      }
    }
    fetchAll()
  }, [])

  const handlePurge = async () => {
    if (!window.confirm('This will permanently delete all soft-deleted resume files. Continue?')) return
    setPurging(true)
    try {
      const res = await api.delete('/admin/purge-resumes')
      setMessage({ type: 'success', text: res.data.message })
    } catch (err) {
      setMessage({ type: 'danger', text: err.response?.data?.message || 'Purge failed.' })
    } finally {
      setPurging(false)
    }
  }

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary" /></div>

  return (
    <div>
      <h4 className="mb-4">Admin Dashboard</h4>
      {message && <div className={`alert alert-${message.type}`}>{message.text}</div>}

      {/* Stats Cards */}
      {stats && (
        <div className="row g-3 mb-4">
          <div className="col-md-3">
            <div className="card text-white bg-primary shadow-sm">
              <div className="card-body text-center">
                <h2 className="fw-bold">{stats.totalJobSeekers}</h2>
                <p className="mb-0">Job Seekers</p>
              </div>
            </div>
          </div>
          <div className="col-md-3">
            <div className="card text-white bg-success shadow-sm">
              <div className="card-body text-center">
                <h2 className="fw-bold">{stats.totalEmployers}</h2>
                <p className="mb-0">Employers</p>
              </div>
            </div>
          </div>
          <div className="col-md-3">
            <div className="card text-white bg-info shadow-sm">
              <div className="card-body text-center">
                <h2 className="fw-bold">{stats.totalActiveJobs}</h2>
                <p className="mb-0">Active Jobs</p>
              </div>
            </div>
          </div>
          <div className="col-md-3">
            <div className="card text-white bg-warning shadow-sm">
              <div className="card-body text-center">
                <h2 className="fw-bold">{stats.totalApplications}</h2>
                <p className="mb-0">Applications</p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Tabs */}
      <ul className="nav nav-tabs mb-4">
        {['users', 'jobs', 'overview'].map(tab => (
          <li key={tab} className="nav-item">
            <button
              className={`nav-link ${activeTab === tab ? 'active' : ''}`}
              onClick={() => setActiveTab(tab)}>
              {tab.charAt(0).toUpperCase() + tab.slice(1)}
            </button>
          </li>
        ))}
      </ul>

      {/* Overview Tab */}
      {activeTab === 'overview' && (
        <div className="card p-4 shadow-sm">
          <h6 className="mb-3">Platform Maintenance</h6>
          <p className="text-muted">
            Soft-deleted resumes are hidden from job seekers but their files still exist
            on the server. Use the button below to permanently remove them.
          </p>
          <button
            className="btn btn-danger"
            onClick={handlePurge}
            disabled={purging}
            style={{ width: 'fit-content' }}>
            {purging ? 'Purging...' : '🗑 Purge Deleted Resume Files'}
          </button>
        </div>
      )}

      {/* Users Tab */}
      {activeTab === 'users' && (
        <div className="card shadow-sm">
          <div className="card-body">
            <h6 className="mb-3">All Users ({users.length})</h6>
            <div className="table-responsive">
              <table className="table table-hover align-middle">
                <thead className="table-light">
                  <tr>
                    <th>#</th>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Role</th>
                    <th>Joined</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((user, index) => (
                    <tr key={user.userId}>
                      <td>{index + 1}</td>
                      <td>{user.fullName}</td>
                      <td>{user.email}</td>
                      <td>
                        <span className={`badge ${
                          user.role === 'Employer' ? 'bg-success' :
                          user.role === 'Admin' ? 'bg-danger' : 'bg-primary'
                        }`}>
                          {user.role}
                        </span>
                      </td>
                      <td>{new Date(user.createdAt).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Jobs Tab */}
      {activeTab === 'jobs' && (
        <div className="card shadow-sm">
          <div className="card-body">
            <h6 className="mb-3">All Job Listings ({jobs.length})</h6>
            <div className="table-responsive">
              <table className="table table-hover align-middle">
                <thead className="table-light">
                  <tr>
                    <th>#</th>
                    <th>Title</th>
                    <th>Company</th>
                    <th>Status</th>
                    <th>Posted</th>
                  </tr>
                </thead>
                <tbody>
                  {jobs.map((job, index) => (
                    <tr key={job.jobId}>
                      <td>{index + 1}</td>
                      <td>{job.title}</td>
                      <td>{job.companyName}</td>
                      <td>
                        <span className={`badge ${job.isActive ? 'bg-success' : 'bg-secondary'}`}>
                          {job.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td>{new Date(job.postedAt).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}