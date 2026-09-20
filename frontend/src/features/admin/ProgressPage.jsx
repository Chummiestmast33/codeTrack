import FormField from '../../components/FormField.jsx'
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
      <h1 className="page-title">{t('progress.title')}</h1>

      <FormField label={t('progress.pickStudent')}><select aria-label={t('progress.pickStudent')} value={userId} onChange={(e) => pick(e.target.value)} className="mt-4 w-full max-w-md field">
        <option value="">{t('progress.pickStudent')}</option>
        {users.map((u) => (
          <option key={u.id} value={u.id}>{u.fullName} · {u.controlNumber}</option>
        ))}
      </select></FormField>

      {error && (
        <p role="alert" className="mt-4 alert alert-danger">
          {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
        </p>
      )}

      {loading && <p role="status" className="state-message">{t('common.loading')}</p>}

      {!loading && userId && rows.length > 0 && (
        <div className="mt-6 space-y-4">
          {rows.map((row) => (
            <article key={row.topicId} className="card">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <p className="font-bold">{row.topicName}</p>
                <span className={`badge ${statusStyle(row.effectiveStatus)}`}>
                  {t(`progress.statuses.${row.effectiveStatus}`)}
                  {row.manualStatus ? ` · ${t('progress.manual')}` : ''}
                </span>
              </div>
              {row.adjustmentReason && <p className="mt-1 text-sm text-muted">{t('progress.reason')}: {row.adjustmentReason}</p>}
              <div className="form-actions">
                <FormField label={t('progress.status')}><select aria-label={t('progress.status')}
                  value={drafts[row.topicId]?.status ?? row.effectiveStatus}
                  onChange={(e) => setDraft(row.topicId, { status: e.target.value })}
                  className="field"
                >
                  {['NotStarted', 'InProgress', 'Completed'].map((s) => (
                    <option key={s} value={s}>{t(`progress.statuses.${s}`)}</option>
                  ))}
                </select></FormField>
                <FormField label={t('progress.reasonPlaceholder')}><input aria-label={t('progress.reasonPlaceholder')}
                  value={drafts[row.topicId]?.reason ?? ''}
                  onChange={(e) => setDraft(row.topicId, { reason: e.target.value })}
                  placeholder={t('progress.reasonPlaceholder')}
                  className="min-w-0 flex-1 field"
                /></FormField>
                <button disabled={busy || !drafts[row.topicId]?.reason?.trim()} onClick={() => onAdjust(row.topicId)} className="btn btn-primary">
                  {t('progress.adjust')}
                </button>
                {row.manualStatus && (
                  <button disabled={busy} onClick={() => onClear(row.topicId)} className="btn">
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
