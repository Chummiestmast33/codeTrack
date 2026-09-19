import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'
import { ApiError } from '../api/client.js'
import LanguageSelector from '../components/LanguageSelector.jsx'

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
    <main className="flex min-h-screen items-center justify-center bg-slate-50 px-6">
      <div className="w-full max-w-sm">
        <div className="mb-4 flex justify-end">
          <LanguageSelector />
        </div>
        <h1 className="text-center text-2xl font-bold">{t('auth.login')}</h1>
        <form onSubmit={onSubmit} className="mt-6 space-y-4 rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-slate-700">{t('auth.controlNumber')}</span>
            <input
              value={controlNumber}
              onChange={(e) => setControlNumber(e.target.value)}
              autoComplete="username"
              className="w-full rounded-md border border-slate-300 px-3 py-2"
            />
          </label>
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-slate-700">{t('auth.password')}</span>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
              className="w-full rounded-md border border-slate-300 px-3 py-2"
            />
          </label>
          {error && (
            <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
              {error.status && t(`errors.${error.status}`, { defaultValue: t('errors.500') })}
            </p>
          )}
          <button
            type="submit"
            disabled={busy}
            className="w-full rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
          >
            {busy ? t('common.loading') : t('auth.login')}
          </button>
        </form>
        <p className="mt-4 text-center text-sm text-slate-600">
          {t('auth.noAccount')}{' '}
          <Link to="/register" className="font-medium text-indigo-600 underline">
            {t('auth.register')}
          </Link>
        </p>
      </div>
    </main>
  )
}
