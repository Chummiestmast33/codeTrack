import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { api, ApiError } from '../../api/client.js'

function actionClass(extra) {
  return `rounded-md border px-2 py-1 text-xs font-medium ${extra}`
}

export default function UsersPage() {
  const { t } = useTranslation()
  const [filter, setFilter] = useState('pending')
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [resetFor, setResetFor] = useState(null)
  const [newPassword, setNewPassword] = useState('')
  const [busy, setBusy] = useState(false)

  const url = filter === 'pending' ? '/api/admin/users/pending' : '/api/admin/users'

  useEffect(() => {
    let cancelled = false
    api(url)
      .then((data) => {
        if (!cancelled) {
          setUsers(data)
          setLoading(false)
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err)
          setLoading(false)
        }
      })
    return () => {
      cancelled = true
    }
  }, [url])

  async function refresh() {
    setLoading(true)
    setError(null)
    try {
      setUsers(await api(url))
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  async function run(path) {
    setBusy(true)
    setError(null)
    try {
      await api(path, { method: 'POST' })
      await refresh()
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  async function runReset(e) {
    e.preventDefault()
    if (!resetFor) return
    setBusy(true)
    setError(null)
    try {
      await api(`/api/admin/users/${resetFor.id}/reset-password`, {
        method: 'POST',
        body: { newPassword },
      })
      setResetFor(null)
      setNewPassword('')
      await refresh()
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  function errorMessage() {
    if (!(error instanceof ApiError)) return t('errors.500')
    return t(`errors.${error.status}`, { defaultValue: t('errors.500') })
  }

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-bold">{t('admin.users.title')}</h1>
        <div className="flex gap-2">
          {['pending', 'all'].map((f) => (
            <button
              key={f}
              onClick={() => { setLoading(true); setFilter(f) }}
              className={`rounded-md px-3 py-1.5 text-sm font-medium ${
                filter === f ? 'bg-slate-900 text-white' : 'border border-slate-300 text-slate-700 hover:bg-slate-100'
              }`}
            >
              {t(`admin.users.filter.${f}`)}
            </button>
          ))}
        </div>
      </div>

      {error && (
        <p role="alert" className="mt-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
          {errorMessage()}
        </p>
      )}

      {loading ? (
        <p className="mt-6 text-slate-600">{t('common.loading')}</p>
      ) : users.length === 0 ? (
        <p className="mt-6 text-slate-600">{t('admin.users.empty')}</p>
      ) : (
        <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white shadow-sm">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 text-slate-500">
              <tr>
                <th className="px-4 py-2">{t('auth.controlNumber')}</th>
                <th className="px-4 py-2">{t('auth.fullName')}</th>
                <th className="px-4 py-2">{t('auth.email')}</th>
                <th className="px-4 py-2">{t('admin.users.status')}</th>
                <th className="px-4 py-2">{t('admin.users.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id} className="border-t border-slate-100">
                  <td className="px-4 py-2 font-mono">{u.controlNumber}</td>
                  <td className="px-4 py-2">{u.fullName}</td>
                  <td className="px-4 py-2">{u.email}</td>
                  <td className="px-4 py-2">
                    <span
                      className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                        u.approvalStatus === 'Approved' && u.isActive
                          ? 'bg-green-100 text-green-800'
                          : 'bg-amber-100 text-amber-800'
                      }`}
                    >
                      {t(`admin.users.approval.${u.approvalStatus}`, { defaultValue: u.approvalStatus })}
                      {u.approvalStatus === 'Approved' && !u.isActive ? ` · ${t('admin.users.inactive')}` : ''}
                    </span>
                  </td>
                  <td className="px-4 py-2">
                    <div className="flex flex-wrap gap-1.5">
                      {u.approvalStatus === 'Pending' && (
                        <>
                          <button disabled={busy} onClick={() => run(`/api/admin/users/${u.id}/approve`)} className={actionClass('border-green-300 text-green-700 hover:bg-green-50')}>
                            {t('admin.users.approve')}
                          </button>
                          <button disabled={busy} onClick={() => run(`/api/admin/users/${u.id}/reject`)} className={actionClass('border-red-300 text-red-700 hover:bg-red-50')}>
                            {t('admin.users.reject')}
                          </button>
                        </>
                      )}
                      {u.isActive ? (
                        <button disabled={busy} onClick={() => run(`/api/admin/users/${u.id}/deactivate`)} className={actionClass('border-slate-300 text-slate-700 hover:bg-slate-100')}>
                          {t('admin.users.deactivate')}
                        </button>
                      ) : (
                        <button disabled={busy} onClick={() => run(`/api/admin/users/${u.id}/activate`)} className={actionClass('border-slate-300 text-slate-700 hover:bg-slate-100')}>
                          {t('admin.users.activate')}
                        </button>
                      )}
                      <button disabled={busy} onClick={() => { setResetFor(u); setNewPassword('') }} className={actionClass('border-slate-300 text-slate-700 hover:bg-slate-100')}>
                        {t('admin.users.resetPassword')}
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {resetFor && (
        <form onSubmit={runReset} className="mt-4 max-w-sm rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
          <h2 className="font-bold">{t('admin.users.resetTitle', { name: resetFor.fullName })}</h2>
          <label className="mt-3 block">
            <span className="mb-1 block text-sm font-medium text-slate-700">{t('auth.password')}</span>
            <input
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              autoComplete="new-password"
              className="w-full rounded-md border border-slate-300 px-3 py-2"
            />
          </label>
          <div className="mt-3 flex gap-2">
            <button type="submit" disabled={busy} className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50">
              {t('common.save')}
            </button>
            <button type="button" onClick={() => setResetFor(null)} className="rounded-md border border-slate-300 px-4 py-2 text-sm text-slate-700 hover:bg-slate-100">
              {t('common.cancel')}
            </button>
          </div>
        </form>
      )}
    </div>
  )
}
