import { useCallback, useEffect, useState } from 'react'
import { useParams } from 'react-router'
import { useTranslation } from 'react-i18next'
import { getActivity } from '../../api/activities.js'
import { reviewSubmission, submissionsByActivity } from '../../api/deliveries.js'
import { ApiError } from '../../api/client.js'
import SafeMarkdown from '../../components/SafeMarkdown.jsx'
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

  if (loading) return <p className="text-slate-600">{t('common.loading')}</p>
  if (error && !activity) {
    return (
      <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
        {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
      </p>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">{activity.title}</h1>
        <p className="text-sm text-slate-500">{activity.topicName} · {t(`activities.statuses.${activity.status}`, { defaultValue: activity.status })}</p>
      </div>

      <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <SafeMarkdown text={activity.markdownContent} />
      </section>

      {error && (
        <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
          {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
        </p>
      )}

      <section>
        <h2 className="font-bold">{t('submissions.title', { count: rows.length })}</h2>
        {rows.length === 0 ? (
          <p className="mt-2 text-slate-600">{t('submissions.empty')}</p>
        ) : (
          <div className="mt-3 space-y-3">
            {rows.map((row) => (
              <article key={row.id} className="rounded-lg border border-slate-200 bg-white p-4 text-sm shadow-sm">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <p className="font-bold">{row.fullName} <span className="font-mono font-normal text-slate-500">{row.controlNumber}</span></p>
                  <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${row.status === 'Reviewed' ? 'bg-green-100 text-green-800' : row.status === 'Incomplete' ? 'bg-red-100 text-red-800' : 'bg-amber-100 text-amber-800'}`}>
                    {t(`submissions.statuses.${row.status}`)} · v{row.versionNumber}
                    {row.isLate ? ` · ${t('submissions.late')}` : ''}
                  </span>
                </div>
                {row.url && <a href={row.url} target="_blank" rel="noreferrer" className="mt-1 block break-all text-indigo-600 underline">{row.url}</a>}
                {row.fileName && <p className="mt-1 text-slate-600">{row.fileName}{row.fileSizeBytes ? ` (${Math.round(row.fileSizeBytes / 1024)} KB)` : ''}</p>}
                {row.comment && <p className="mt-1 italic text-slate-600">“{row.comment}”</p>}
                {row.instructorComment && <p className="mt-1 text-slate-700">{t('submissions.instructorComment')}: {row.instructorComment}</p>}
                <p className="mt-1 text-xs text-slate-400">{formatDateTime(row.submittedAt, lang)}</p>
                <div className="mt-3 flex flex-wrap items-center gap-2">
                  <select
                    value={review[row.id]?.status ?? 'Reviewed'}
                    onChange={(e) => setDraft(row.id, { status: e.target.value })}
                    className="rounded-md border border-slate-300 px-2 py-1.5 text-xs"
                  >
                    <option value="Reviewed">{t('submissions.statuses.Reviewed')}</option>
                    <option value="Incomplete">{t('submissions.statuses.Incomplete')}</option>
                  </select>
                  <input
                    value={review[row.id]?.comment ?? ''}
                    onChange={(e) => setDraft(row.id, { comment: e.target.value })}
                    placeholder={t('submissions.commentPlaceholder')}
                    className="min-w-52 flex-1 rounded-md border border-slate-300 px-2 py-1.5 text-xs"
                  />
                  <button disabled={busy} onClick={() => onReview(row)} className="rounded-md bg-indigo-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-indigo-500 disabled:opacity-50">
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
