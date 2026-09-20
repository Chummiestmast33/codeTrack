import { useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'
import { ApiError } from '../api/client.js'
import AuthLayout from '../components/AuthLayout.jsx'

export default function Register() {
  const { t } = useTranslation()
  const { register } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ controlNumber: '', fullName: '', email: '', password: '' })
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)

  function set(field, value) {
    setForm((f) => ({ ...f, [field]: value }))
  }

  async function onSubmit(e) {
    e.preventDefault()
    setError(null)
    setBusy(true)
    try {
      await register({
        controlNumber: form.controlNumber.trim(),
        fullName: form.fullName.trim(),
        email: form.email.trim(),
        password: form.password,
      })
      navigate('/pending', { replace: true })
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  function errorMessage() {
    if (!(error instanceof ApiError)) return t('errors.500')
    if (error.status === 400 && error.errors) {
      const first = Object.values(error.errors).flat()[0]
      if (first) return first
    }
    return t(`errors.${error.status}`, { defaultValue: t('errors.500') })
  }

  return (
    <AuthLayout mode="register">
        <form onSubmit={onSubmit} className="auth-form">
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-ink">{t('auth.controlNumber')}</span>
            <input
              value={form.controlNumber}
              onChange={(e) => set('controlNumber', e.target.value)}
              autoComplete="username"
              className="field"
            />
          </label>
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-ink">{t('auth.fullName')}</span>
            <input
              value={form.fullName}
              onChange={(e) => set('fullName', e.target.value)}
              autoComplete="name"
              className="field"
            />
          </label>
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-ink">{t('auth.email')}</span>
            <input
              type="email"
              value={form.email}
              onChange={(e) => set('email', e.target.value)}
              autoComplete="email"
              className="field"
            />
          </label>
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-ink">{t('auth.password')}</span>
            <input
              type="password"
              value={form.password}
              onChange={(e) => set('password', e.target.value)}
              autoComplete="new-password"
              className="field"
            />
          </label>
          {error && (
            <p role="alert" className="alert alert-danger">
              {errorMessage()}
            </p>
          )}
          <button
            type="submit"
            disabled={busy}
            className="w-full btn btn-primary"
          >
            {busy ? t('common.loading') : t('auth.register')}
          </button>
        </form>
        <p className="mt-4 text-center text-sm text-muted">
          {t('auth.hasAccount')}{' '}
          <Link to="/login" className="font-medium text-accent underline">
            {t('auth.login')}
          </Link>
        </p>
    </AuthLayout>
  )
}
