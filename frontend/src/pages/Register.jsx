import { useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'
import { ApiError } from '../api/client.js'
import LanguageSelector from '../components/LanguageSelector.jsx'

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
    <main className="flex min-h-screen items-center justify-center bg-slate-50 px-6">
      <div className="w-full max-w-sm">
        <div className="mb-4 flex justify-end">
          <LanguageSelector />
        </div>
        <h1 className="text-center text-2xl font-bold">{t('auth.register')}</h1>
        <form onSubmit={onSubmit} className="mt-6 space-y-4 rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-slate-700">{t('auth.controlNumber')}</span>
            <input
              value={form.controlNumber}
              onChange={(e) => set('controlNumber', e.target.value)}
              autoComplete="username"
              className="w-full rounded-md border border-slate-300 px-3 py-2"
            />
          </label>
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-slate-700">{t('auth.fullName')}</span>
            <input
              value={form.fullName}
              onChange={(e) => set('fullName', e.target.value)}
              autoComplete="name"
              className="w-full rounded-md border border-slate-300 px-3 py-2"
            />
          </label>
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-slate-700">{t('auth.email')}</span>
            <input
              type="email"
              value={form.email}
              onChange={(e) => set('email', e.target.value)}
              autoComplete="email"
              className="w-full rounded-md border border-slate-300 px-3 py-2"
            />
          </label>
          <label className="block">
            <span className="mb-1 block text-sm font-medium text-slate-700">{t('auth.password')}</span>
            <input
              type="password"
              value={form.password}
              onChange={(e) => set('password', e.target.value)}
              autoComplete="new-password"
              className="w-full rounded-md border border-slate-300 px-3 py-2"
            />
          </label>
          {error && (
            <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
              {errorMessage()}
            </p>
          )}
          <button
            type="submit"
            disabled={busy}
            className="w-full rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
          >
            {busy ? t('common.loading') : t('auth.register')}
          </button>
        </form>
        <p className="mt-4 text-center text-sm text-slate-600">
          {t('auth.hasAccount')}{' '}
          <Link to="/login" className="font-medium text-indigo-600 underline">
            {t('auth.login')}
          </Link>
        </p>
      </div>
    </main>
  )
}
