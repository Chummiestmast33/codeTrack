import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import { api, getToken, setToken } from '../../api/client.js'

const AuthContext = createContext(null)

/** Role comes from the JWT payload for UI guards only; the backend revalidates everything. */
function roleFromToken(token) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))
    return payload.role ?? payload[Object.keys(payload).find((k) => k.endsWith('/role'))] ?? null
  } catch {
    return null
  }
}

export function AuthProvider({ children }) {
  const [token, setTokenState] = useState(() => getToken())
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(!!getToken())

  const logout = useCallback(() => {
    setToken(null)
    setTokenState(null)
    setUser(null)
  }, [])

  useEffect(() => {
    if (!token) return
    let cancelled = false
    api('/api/users/me', { token })
      .then((me) => {
        if (!cancelled) {
          setUser(me)
          setLoading(false)
        }
      })
      .catch(() => {
        if (!cancelled) {
          setToken(null)
          setTokenState(null)
          setUser(null)
          setLoading(false)
        }
      })
    return () => {
      cancelled = true
    }
  }, [token])

  const login = useCallback(async (controlNumber, password) => {
    const result = await api('/api/auth/login', {
      method: 'POST',
      body: { controlNumber, password },
    })
    setToken(result.token)
    setTokenState(result.token)
    setUser(result.user)
    return result.user
  }, [])

  const register = useCallback(async (payload) => {
    const created = await api('/api/auth/register', { method: 'POST', body: payload })
    return created
  }, [])

  const value = useMemo(
    () => ({
      token,
      user,
      loading,
      role: token ? roleFromToken(token) : null,
      isApproved: user?.approvalStatus === 'Approved' && user?.isActive === true,
      login,
      register,
      logout,
      refresh: async () => {
        if (!token) return
        setUser(await api('/api/users/me', { token }))
      },
    }),
    [token, user, loading, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components -- hook colocated with its context
export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
