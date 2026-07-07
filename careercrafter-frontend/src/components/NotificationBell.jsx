import { useState, useEffect } from 'react'
import { FaBell } from 'react-icons/fa'
import api from '../services/api'

export default function NotificationBell() {
  const [notifications, setNotifications] = useState([])
  const [loading, setLoading] = useState(true)

  const fetchNotifications = async () => {
    try {
      const res = await api.get('/notification')
      setNotifications(res.data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchNotifications()
    const interval = setInterval(fetchNotifications, 30000) // poll every 30s
    return () => clearInterval(interval)
  }, [])

  const unreadCount = notifications.filter(n => !n.isRead).length

  const handleMarkAsRead = async (notificationId) => {
    try {
      await api.patch(`/notification/${notificationId}/mark-read`)
      setNotifications(notifications.map(n =>
        n.notificationId === notificationId ? { ...n, isRead: true } : n
      ))
    } catch (err) {
      console.error(err)
    }
  }

  return (
    <div className="dropdown">
      <button
        className="btn btn-outline-light btn-sm position-relative"
        type="button"
        data-bs-toggle="dropdown"
        aria-expanded="false"
      >
        {/* 🔔 */}
        <FaBell size={18} className="mb-1" /> 
        {unreadCount > 0 && (
          <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
            {unreadCount}
            <span className="visually-hidden">unread notifications</span>
          </span>
        )}
      </button>
      <ul className="dropdown-menu dropdown-menu-end shadow"
        style={{ width: '320px', maxHeight: '400px', overflowY: 'auto' }}>
        <li><h6 className="dropdown-header">Notifications</h6></li>
        {loading ? (
          <li className="text-center py-3">
            <div className="spinner-border spinner-border-sm text-primary" />
          </li>
        ) : notifications.length === 0 ? (
          <li className="text-center text-muted py-3 px-3">No notifications yet.</li>
        ) : (
          notifications.map(n => (
            <li key={n.notificationId}>
              <button
                className="dropdown-item py-2"
                style={{ whiteSpace: 'normal' }}
                onClick={() => !n.isRead && handleMarkAsRead(n.notificationId)}
              >
                <div className={n.isRead ? 'text-muted' : 'fw-bold'}>
                  {n.message}
                </div>
                <small className="text-muted">
                  {new Date(n.createdAt).toLocaleString()}
                </small>
              </button>
            </li>
          ))
        )}
      </ul>
    </div>
  )
}