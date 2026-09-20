import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { adjustProgress, clearProgressAdjustment, studentProgress } from '../../api/deliveries.js'
import { api, ApiError } from '../../api/client.js'
import { statusStyle } from '../../utils/statusStyle.js'

export default function ProgressPage() {
  const { t } = useTranslation()
  const [users, setUsers] = useState([])
  const [userId, setUserId] = useState('')
  const [rows, setRows] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)
  const [drafts, setDrafts] = useState({})

  useEffect(() => {
    let cancelled = false
    api('/api/admin/users')
      .then((data) => {
        if (!cancelled) setUsers(data.filter((u) => u.role === 'Student'))
      })
      .catch((err) => {
        if (!cancelled) setError(err)
      })
    return () => {
      cancelled = true
    }
  }, [])

  const load = useCallback(async (id) => {
    setLoading(true)
    setError(null)
    try {
      setRows(await studentProgress(id))
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }, [])

  function pick(id) {
    setUserId(id)
    setDrafts({})
    if (id) load(id).catch(() => {})
  }

  async function onAdjust(topicId) {
    const draft = drafts[topicId]
    if (!draft?.reason?.trim()) return
    setBusy(true)
    setError(null)
    try {
      await adjustProgress(userId, topicId, { status: draft.status, reason: draft.reason.trim() })
      setDrafts((d) => ({ ...d, [topicId]: undefined }))
      await load(userId)
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  async function onClear(topicId) {
    setBusy(true)
    setError(null)
    try {
      await clearProgressAdjustment(userId, topicId)
      await load(userId)
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  function setDraft(topicId, patch) {
    setDrafts((d) => ({ ...d, [topicId]: { status: 'Completed', reason: '', ...d[topicId], ...patch } }))
  }

  return (
    <div>
      <h1 className="text-2xl font-bold">{t('progress.title')}</h1>

      <select value={userId} onChange={(e) => pick(e.target.value)} className="mt-4 w-full max-w-md rounded-md border border-slate-300 px-3 py-2">
        <option value="">{t('progress.pickStudent')}</option>
        {users.map((u) => (
          <option key={u.id} value={u.id}>{u.fullName} · {u.controlNumber}</option>
        ))}
      </select>

      {error && (
        <p role="alert" className="mt-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
          {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
        </p>
      )}

      {loading && <p className="mt-4 text-slate-600">{t('common.loading')}</p>}

      {!loading && userId && rows.length > 0 && (
        <div className="mt-4 space-y-3">
          {rows.map((row) => (
            <article key={row.topicId} className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <p className="font-bold">{row.topicName}</p>
                <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${statusStyle(row.effectiveStatus)}`}>
                  {t(`progress.statuses.${row.effectiveStatus}`)}
                  {row.manualStatus ? ` · ${t('progress.manual')}` : ''}
                </span>
              </div>
              {row.adjustmentReason && <p className="mt-1 text-sm text-slate-600">{t('progress.reason')}: {row.adjustmentReason}</p>}
              <div className="mt-3 flex flex-wrap items-center gap-2">
                <select
                  value={drafts[row.topicId]?.status ?? row.effectiveStatus}
                  onChange={(e) => setDraft(row.topicId, { status: e.target.value })}
                  className="rounded-md border border-slate-300 px-2 py-1.5 text-xs"
                >
                  {['NotStarted', 'InProgress', 'Completed'].map((s) => (
                    <option key={s} value={s}>{t(`progress.statuses.${s}`)}</option>
                  ))}
                </select>
                <input
                  value={drafts[row.topicId]?.reason ?? ''}
                  onChange={(e) => setDraft(row.topicId, { reason: e.target.value })}
                  placeholder={t('progress.reasonPlaceholder')}
                  className="min-w-52 flex-1 rounded-md border border-slate-300 px-2 py-1.5 text-xs"
                />
                <button disabled={busy || !drafts[row.topicId]?.reason?.trim()} onClick={() => onAdjust(row.topicId)} className="rounded-md bg-indigo-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-indigo-500 disabled:opacity-50">
                  {t('progress.adjust')}
                </button>
                {row.manualStatus && (
                  <button disabled={busy} onClick={() => onClear(row.topicId)} className="rounded-md border border-slate-300 px-3 py-1.5 text-xs hover:bg-slate-100">
                    {t('progress.clear')}
                  </button>
                )}
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  )
}
