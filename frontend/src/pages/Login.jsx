import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'
import { ApiError } from '../api/client.js'
import AuthLayout from '../components/AuthLayout.jsx'

export default function Login() {
  const { t } = useTranslation()
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [controlNumber, setControlNumber] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)

  async function onSubmit(e) {
    e.preventDefault()
    setError(null)
    setBusy(true)
    try {
      const user = await login(controlNumber.trim(), password)
      const from = location.state?.from
      if (from && from !== '/login' && from !== '/register') {
        navigate(from, { replace: true })
      } else {
        navigate(user.role === 'Administrator' ? '/admin/users' : '/app', { replace: true })
      }
    } catch (err) {
      if (err instanceof ApiError && err.status === 403) {
        navigate('/pending', { replace: true })
        return
      }
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  return (
    <AuthLayout mode="login">
        <form onSubmit={onSubmit} className="auth-form">
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-ink">{t('auth.controlNumber')}</span>
            <input
              value={controlNumber}
              onChange={(e) => setControlNumber(e.target.value)}
              autoComplete="username"
              className="field"
            />
          </label>
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-ink">{t('auth.password')}</span>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
              className="field"
            />
          </label>
          {error && (
            <p role="alert" className="alert alert-danger">
              {t(`errors.${error.status}`, { defaultValue: t('errors.500') })}
            </p>
          )}
          <button
            type="submit"
            disabled={busy}
            className="w-full btn btn-primary"
          >
            {busy ? t('common.loading') : t('auth.login')}
          </button>
        </form>
        <p className="mt-4 text-center text-sm text-muted">
          {t('auth.noAccount')}{' '}
          <Link to="/register" className="font-medium text-accent underline">
            {t('auth.register')}
          </Link>
        </p>
    </AuthLayout>
  )
}
