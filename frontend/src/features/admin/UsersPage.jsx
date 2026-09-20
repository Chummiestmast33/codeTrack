import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { api, ApiError } from '../../api/client.js'

function actionClass(extra) {
  return `btn ${extra}`
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
        <h1 className="page-title">{t('admin.users.title')}</h1>
        <div className="flex flex-wrap gap-2">
          {['pending', 'all'].map((f) => (
            <button
              key={f}
              onClick={() => { setLoading(true); setFilter(f) }}
              className={`btn ${
                filter === f ? 'btn-primary' : ''
              }`}
            >
              {t(`admin.users.filter.${f}`)}
            </button>
          ))}
        </div>
      </div>

      {error && (
        <p role="alert" className="mt-4 alert alert-danger">
          {errorMessage()}
        </p>
      )}

      {loading ? (
        <p role="status" className="state-message">{t('common.loading')}</p>
      ) : users.length === 0 ? (
        <p role="status" className="state-message">{t('admin.users.empty')}</p>
      ) : (
        <div tabIndex={0} role="region" aria-label={t('common.tableRegion')} className="mt-6 table-panel">
          <table className="w-full text-left text-sm">
            <thead className="bg-canvas text-muted">
              <tr>
                <th>{t('auth.controlNumber')}</th>
                <th>{t('auth.fullName')}</th>
                <th>{t('auth.email')}</th>
                <th>{t('admin.users.status')}</th>
                <th>{t('admin.users.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id} className="border-t border-line">
                  <td className="font-mono">{u.controlNumber}</td>
                  <td>{u.fullName}</td>
                  <td>{u.email}</td>
                  <td>
                    <span
                      className={`badge ${
                        u.approvalStatus === 'Approved' && u.isActive
                          ? 'bg-success-soft text-success'
                          : 'bg-warning-soft text-warning'
                      }`}
                    >
                      {t(`admin.users.approval.${u.approvalStatus}`, { defaultValue: u.approvalStatus })}
                      {u.approvalStatus === 'Approved' && !u.isActive ? ` · ${t('admin.users.inactive')}` : ''}
                    </span>
                  </td>
                  <td>
                    <div className="flex flex-wrap gap-2">
                      {u.approvalStatus === 'Pending' && (
                        <>
                          <button disabled={busy} onClick={() => run(`/api/admin/users/${u.id}/approve`)} className={actionClass('btn-success')}>
                            {t('admin.users.approve')}
                          </button>
                          <button disabled={busy} onClick={() => run(`/api/admin/users/${u.id}/reject`)} className={actionClass('btn-danger')}>
                            {t('admin.users.reject')}
                          </button>
                        </>
                      )}
                      {u.isActive ? (
                        <button disabled={busy} onClick={() => run(`/api/admin/users/${u.id}/deactivate`)} className={actionClass('')}>
                          {t('admin.users.deactivate')}
                        </button>
                      ) : (
                        <button disabled={busy} onClick={() => run(`/api/admin/users/${u.id}/activate`)} className={actionClass('')}>
                          {t('admin.users.activate')}
                        </button>
                      )}
                      <button disabled={busy} onClick={() => { setResetFor(u); setNewPassword('') }} className={actionClass('')}>
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
        <form onSubmit={runReset} className="mt-4 max-w-sm card">
          <h2 className="font-bold">{t('admin.users.resetTitle', { name: resetFor.fullName })}</h2>
          <label className="mt-3 block">
            <span className="mb-2 block text-sm font-medium text-ink">{t('auth.password')}</span>
            <input
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              autoComplete="new-password"
              className="field"
            />
          </label>
          <div className="mt-3 flex flex-wrap gap-2">
            <button type="submit" disabled={busy} className="btn btn-primary">
              {t('common.save')}
            </button>
            <button type="button" onClick={() => setResetFor(null)} className="btn">
              {t('common.cancel')}
            </button>
          </div>
        </form>
      )}
    </div>
  )
}
