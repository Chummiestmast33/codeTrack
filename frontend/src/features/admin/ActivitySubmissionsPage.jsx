import FormField from '../../components/FormField.jsx'
import { useCallback, useEffect, useState } from 'react'
import { useParams } from 'react-router'
import { useTranslation } from 'react-i18next'
import { getActivity } from '../../api/activities.js'
import { reviewSubmission, submissionsByActivity } from '../../api/deliveries.js'
import { ApiError } from '../../api/client.js'
import SafeMarkdown from '../../components/SafeMarkdown.jsx'
import SubmissionFileActions from './SubmissionFileActions.jsx'
import { formatDateTime } from '../../utils/format.js'
import { getLanguage } from '../../i18n.js'

export default function ActivitySubmissionsPage() {
  const { t, i18n } = useTranslation()
  const { id } = useParams()
  const lang = getLanguage().split('-')[0] || i18n.language
  const [activity, setActivity] = useState(null)
  const [rows, setRows] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)
  const [review, setReview] = useState({})

  const load = useCallback(async () => {
    const [a, list] = await Promise.all([getActivity(id), submissionsByActivity(id)])
    return { activity: a, rows: list }
  }, [id])

  useEffect(() => {
    let cancelled = false
    load()
      .then(({ activity: a, rows: list }) => {
        if (!cancelled) {
          setActivity(a)
          setRows(list)
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
  }, [load])

  async function refresh() {
    setError(null)
    try {
      const { activity: a, rows: list } = await load()
      setActivity(a)
      setRows(list)
    } catch (err) {
      setError(err)
    }
  }

  async function onReview(row) {
    const draft = review[row.id] ?? { status: 'Reviewed', comment: '' }
    setBusy(true)
    setError(null)
    try {
      await reviewSubmission(row.id, { status: draft.status, instructorComment: draft.comment || null })
      setReview((r) => ({ ...r, [row.id]: undefined }))
      await refresh()
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  function setDraft(rowId, patch) {
    setReview((r) => ({ ...r, [rowId]: { status: 'Reviewed', comment: '', ...r[rowId], ...patch } }))
  }

  if (loading) return <p role="status" className="state-message">{t('common.loading')}</p>
  if (error && !activity) {
    return (
      <p role="alert" className="alert alert-danger">
        {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
      </p>
    )
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="page-title">{activity.title}</h1>
        <p className="text-sm text-muted">{activity.topicName} · {t(`activities.statuses.${activity.status}`, { defaultValue: activity.status })}</p>
      </div>

      <section className="card">
        <SafeMarkdown text={activity.markdownContent} />
      </section>

      {error && (
        <p role="alert" className="alert alert-danger">
          {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
        </p>
      )}

      <section>
        <h2 className="font-bold">{t('submissions.title', { count: rows.length })}</h2>
        {rows.length === 0 ? (
          <p role="status" className="state-message">{t('submissions.empty')}</p>
        ) : (
          <div className="mt-6 space-y-6">
            {rows.map((row) => (
              <article key={row.id} className="card text-sm">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <p className="font-bold">{row.fullName} <span className="font-mono font-normal text-muted">{row.controlNumber}</span></p>
                  <span className={`badge ${row.status === 'Reviewed' ? 'bg-success-soft text-success' : row.status === 'Incomplete' ? 'bg-danger-soft text-danger' : 'bg-warning-soft text-warning'}`}>
                    {t(`submissions.statuses.${row.status}`)} · v{row.versionNumber}
                    {row.isLate ? ` · ${t('submissions.late')}` : ''}
                  </span>
                </div>
                {row.url && <a href={row.url} target="_blank" rel="noreferrer" className="mt-1 block break-all text-accent underline">{row.url}</a>}
                {row.fileName && <p className="mt-1 text-muted">{row.fileName}{row.fileSizeBytes ? ` (${Math.round(row.fileSizeBytes / 1024)} KB)` : ''}</p>}
                <SubmissionFileActions submission={row} />
                {row.comment && <p className="mt-1 italic text-muted">“{row.comment}”</p>}
                {row.instructorComment && <p className="mt-1 text-ink">{t('submissions.instructorComment')}: {row.instructorComment}</p>}
                <p className="mt-1 text-sm text-muted">{formatDateTime(row.submittedAt, lang)}</p>
                <div className="form-actions">
                  <FormField label={t('submissions.statusLabel')}><select aria-label={t('submissions.statusLabel')}
                    value={review[row.id]?.status ?? 'Reviewed'}
                    onChange={(e) => setDraft(row.id, { status: e.target.value })}
                    className="field"
                  >
                    <option value="Reviewed">{t('submissions.statuses.Reviewed')}</option>
                    <option value="Incomplete">{t('submissions.statuses.Incomplete')}</option>
                  </select></FormField>
                  <FormField label={t('submissions.commentPlaceholder')}><input aria-label={t('submissions.commentPlaceholder')}
                    value={review[row.id]?.comment ?? ''}
                    onChange={(e) => setDraft(row.id, { comment: e.target.value })}
                    placeholder={t('submissions.commentPlaceholder')}
                    className="min-w-0 flex-1 field"
                  /></FormField>
                  <button disabled={busy} onClick={() => onReview(row)} className="btn btn-primary">
                    {t('submissions.review')}
                  </button>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}
