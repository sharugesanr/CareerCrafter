import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../services/api'

export default function JobSearch() {
  const navigate = useNavigate()
  const [jobs, setJobs] = useState([])
  const [loading, setLoading] = useState(false)
  const [filters, setFilters] = useState({
    title: '', location: '', jobType: '', companyName: '', sortBy: 'newest', page: 1, pageSize: 10
  })
  const [pagination, setPagination] = useState({ totalCount: 0, totalPages: 0 })

  const fetchJobs = async (currentFilters) => {
    setLoading(true)
    try {
      const params = new URLSearchParams()
      if (currentFilters.title) params.append('title', currentFilters.title)
      if (currentFilters.location) params.append('location', currentFilters.location)
      if (currentFilters.jobType) params.append('jobType', currentFilters.jobType)
      if (currentFilters.companyName) params.append('companyName', currentFilters.companyName)
      params.append('sortBy', currentFilters.sortBy)
      params.append('page', currentFilters.page)
      params.append('pageSize', currentFilters.pageSize)

      const res = await api.get(`/jobs?${params.toString()}`)
      setJobs(res.data.items)
      setPagination({ totalCount: res.data.totalCount, totalPages: res.data.totalPages })
    } catch (err) {
      console.error('Failed to fetch jobs', err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchJobs(filters)
  }, [])

  const handleSearch = (e) => {
    e.preventDefault()
    const updated = { ...filters, page: 1 }
    setFilters(updated)
    fetchJobs(updated)
  }

  const handleFilterChange = (e) => {
    setFilters({ ...filters, [e.target.name]: e.target.value })
  }

  const handlePageChange = (newPage) => {
    const updated = { ...filters, page: newPage }
    setFilters(updated)
    fetchJobs(updated)
  }

  return (
    <div>
      <h4 className="mb-4">Search Jobs</h4>

      {/* Filters */}
      <form onSubmit={handleSearch} className="card p-3 mb-4">
        <div className="row g-2">
          <div className="col-md-3">
            <input type="text" name="title" className="form-control"
              placeholder="Job title" value={filters.title} onChange={handleFilterChange} />
          </div>
          <div className="col-md-3">
            <input type="text" name="location" className="form-control"
              placeholder="Location" value={filters.location} onChange={handleFilterChange} />
          </div>
          <div className="col-md-2">
            <select name="jobType" className="form-select" value={filters.jobType} onChange={handleFilterChange}>
              <option value="">All Types</option>
              <option value="Full-time">Full-time</option>
              <option value="Part-time">Part-time</option>
              <option value="Contract">Contract</option>
              <option value="Internship">Internship</option>
            </select>
          </div>
          <div className="col-md-2">
            <select name="sortBy" className="form-select" value={filters.sortBy} onChange={handleFilterChange}>
              <option value="newest">Newest First</option>
              <option value="oldest">Oldest First</option>
            </select>
          </div>
          <div className="col-md-2">
            <button type="submit" className="btn btn-primary w-100">Search</button>
          </div>
        </div>
      </form>

      {/* Results */}
      {loading ? (
        <div className="text-center py-5"><div className="spinner-border text-primary" /></div>
      ) : jobs.length === 0 ? (
        <div className="text-center text-muted py-5">No jobs found.</div>
      ) : (
        <>
          <p className="text-muted mb-3">{pagination.totalCount} jobs found</p>
          {jobs.map(job => (
            <div key={job.jobId} className="card mb-3 shadow-sm">
              <div className="card-body">
                <div className="d-flex justify-content-between align-items-start">
                  <div>
                    <h5 className="card-title mb-1">{job.title}</h5>
                    <p className="text-muted mb-1">{job.companyName} — {job.location}</p>
                    <span className="badge bg-secondary me-2">{job.jobType}</span>
                    <span className="badge bg-light text-dark border">{job.salaryRange}</span>
                  </div>
                  <button className="btn btn-outline-primary btn-sm"
                    onClick={() => navigate(`/jobseeker/jobs/${job.jobId}`)}>
                    View Details
                  </button>
                </div>
                <p className="text-muted small mt-2 mb-0">
                  Posted: {new Date(job.postedAt).toLocaleDateString()}
                  {job.deadline && ` · Deadline: ${new Date(job.deadline).toLocaleDateString()}`}
                </p>
              </div>
            </div>
          ))}

          {/* Pagination */}
          {pagination.totalPages > 1 && (
            <nav className="mt-3">
              <ul className="pagination justify-content-center">
                <li className={`page-item ${filters.page === 1 ? 'disabled' : ''}`}>
                  <button className="page-link" onClick={() => handlePageChange(filters.page - 1)}>Previous</button>
                </li>
                {[...Array(pagination.totalPages)].map((_, i) => (
                  <li key={i} className={`page-item ${filters.page === i + 1 ? 'active' : ''}`}>
                    <button className="page-link" onClick={() => handlePageChange(i + 1)}>{i + 1}</button>
                  </li>
                ))}
                <li className={`page-item ${filters.page === pagination.totalPages ? 'disabled' : ''}`}>
                  <button className="page-link" onClick={() => handlePageChange(filters.page + 1)}>Next</button>
                </li>
              </ul>
            </nav>
          )}
        </>
      )}
    </div>
  )
}