import { createContext, useContext, useReducer } from 'react'

const AuthContext = createContext(null)

const initialState = {
  user: JSON.parse(localStorage.getItem('user')) || null,
  token: localStorage.getItem('token') || null,
  role: localStorage.getItem('role') || null,
}

function authReducer(state, action) {
  switch (action.type) {
    case 'LOGIN':
      localStorage.setItem('token', action.payload.token)
      localStorage.setItem('role', action.payload.role)
      localStorage.setItem('user', JSON.stringify(action.payload.user))
      return {
        token: action.payload.token,
        role: action.payload.role,
        user: action.payload.user,
      }
    case 'LOGOUT':
      localStorage.removeItem('token')
      localStorage.removeItem('role')
      localStorage.removeItem('user')
      return { user: null, token: null, role: null }
    default:
      return state
  }
}

export function AuthProvider({ children }) {
  const [state, dispatch] = useReducer(authReducer, initialState)

  const login = (data) => {
    dispatch({
      type: 'LOGIN',
      payload: {
        token: data.token,
        role: data.role,
        user: { userId: data.userId, fullName: data.fullName, email: data.email },
      },
    })
  }

  const logout = () => {
    dispatch({ type: 'LOGOUT' })
  }

  return (
    <AuthContext.Provider value={{ ...state, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}