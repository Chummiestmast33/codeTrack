import { useCallback, useEffect, useState } from 'react'
import { useParams } from 'react-router'
import { useTranslation } from 'react-i18next'
import { getActivity } from '../../api/activities.js'
import { mySubmissionHistory, requestUploadTicket, submitActivity, uploadToStorage } from '../../api/deliveries.js'
import { ApiError } from '../../api/client.js'
import SafeMarkdown from '../../components/SafeMarkdown.jsx'
import { formatDate, formatDateTime } from '../../utils/format.js'
import { getLanguage } from '../../i18n.js'

export default function ActivityDetailPage() {
  const { t, i18n } = useTranslation()
  const { id } = useParams()
  const lang = getLanguage().split('-')[0] || i18n.language
  const [activity, setActivity] = useState(null)
  const [history, setHistory] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)
  const [form, setForm] = useState({ url: '', file: null, comment: '' })
  const [notice, setNotice] = useState(null)

  const load = useCallback(async () => {
    const [a, h] = await Promise.all([getActivity(id), mySubmissionHistory(id)])
    return { activity: a, history: h }
  }, [id])

  useEffect(() => {
    let cancelled = false
    load()
      .then(({ activity: a, history: h }) => {
        if (!cancelled) {
          setActivity(a)
          setHistory(h)
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

  async function onSubmit(e) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    setNotice(null)
    try {
      let storagePath = null
      let fileName = null
      let contentType = null
      let fileSizeBytes = null

      if (form.file) {
        const ticket = await requestUploadTicket(id, {
          fileName: form.file.name,
          contentType: form.file.type || 'application/octet-stream',
          fileSizeBytes: form.file.size,
        })
        storagePath = await uploadToStorage(ticket, form.file)
        fileName = form.file.name
        contentType = form.file.type || 'application/octet-stream'
        fileSizeBytes = form.file.size
      }

      const created = await submitActivity(id, {
        url: form.url.trim() || null,
        storagePath,
        fileName,
        contentType,
        fileSizeBytes,
        comment: form.comment.trim() || null,
      })
      setForm({ url: '', file: null, comment: '' })
      const { activity: a, history: h } = await load()
      setActivity(a)
      setHistory(h)
      setNotice(created.isLate ? t('submissions.submittedLate') : t('submissions.submitted'))
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  function errorMessage() {
    if (!(error instanceof ApiError)) return error ? t('errors.500') : ''
    if (error.status === 400 && error.errors) {
      const first = Object.values(error.errors).flat()[0]
      if (first) return first
    }
    return t(`errors.${error.status}`, { defaultValue: t('errors.500') })
  }

  const input = 'w-full rounded-md border border-slate-300 px-3 py-2'
  if (loading) return <p className="text-slate-600">{t('common.loading')}</p>
  if (error && !activity) {
    return (
      <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
        {errorMessage()}
      </p>
    )
  }

  const needsUrl = activity.submissionMode === 'UrlOnly' || activity.submissionMode === 'UrlAndFile'
  const needsFile = activity.submissionMode === 'FileOnly' || activity.submissionMode === 'UrlAndFile'
  const allowsFile = needsFile || activity.submissionMode === 'UrlOrFile'
  const current = history.length > 0 ? history[history.length - 1] : null

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">{activity.title}</h1>
        <p className="text-sm text-slate-500">
          {activity.topicName}
          {activity.dueDate ? ` · ${t('activities.due')}: ${formatDate(activity.dueDate, lang)}` : ''}
          {' · '}{t(`activities.modes.${activity.submissionMode}`)}
        </p>
      </div>

      <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <SafeMarkdown text={activity.markdownContent} />
      </section>

      {error && (
        <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
          {errorMessage()}
        </p>
      )}
      {notice && (
        <p role="status" className="rounded-md bg-green-50 px-3 py-2 text-sm text-green-800">
          {notice}
        </p>
      )}

      <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <h2 className="font-bold">{t('submissions.deliver')}</h2>
        <form onSubmit={onSubmit} className="mt-3 grid max-w-xl gap-3">
          {(needsUrl || activity.submissionMode === 'UrlOrFile') && (
            <input value={form.url} onChange={(e) => setForm({ ...form, url: e.target.value })} placeholder="https://…" className={input} />
          )}
          {allowsFile && (
            <input type="file" onChange={(e) => setForm({ ...form, file: e.target.files?.[0] ?? null })} className={input} />
          )}
          <input value={form.comment} onChange={(e) => setForm({ ...form, comment: e.target.value })} placeholder={t('submissions.commentPlaceholder')} className={input} />
          <button disabled={busy} className="w-fit rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50">
            {busy ? t('common.loading') : t('submissions.deliver')}
          </button>
        </form>
        <p className="mt-2 text-xs text-slate-500">{t('submissions.deliveryHint')}</p>
      </section>

      <section>
        <h2 className="font-bold">{t('submissions.history', { count: history.length })}</h2>
        {history.length === 0 ? (
          <p className="mt-2 text-slate-600">{t('submissions.noHistory')}</p>
        ) : (
          <div className="mt-3 space-y-3">
            {current && (
              <article className="rounded-lg border-2 border-indigo-200 bg-white p-4 text-sm shadow-sm">
                <p className="font-bold">{t('submissions.current')} · v{current.versionNumber} · {t(`submissions.statuses.${current.status}`)}{current.isLate ? ` · ${t('submissions.late')}` : ''}</p>
                {current.url && <a href={current.url} target="_blank" rel="noreferrer" className="break-all text-indigo-600 underline">{current.url}</a>}
                {current.fileName && <p className="text-slate-600">{current.fileName}</p>}
                {current.instructorComment && <p className="mt-1 text-slate-700">{t('submissions.instructorComment')}: {current.instructorComment}</p>}
                <p className="mt-1 text-xs text-slate-400">{formatDateTime(current.submittedAt, lang)}</p>
              </article>
            )}
            {history.slice(0, -1).reverse().map((h) => (
              <article key={h.id} className="rounded-lg border border-slate-200 bg-slate-50 p-3 text-sm text-slate-600">
                v{h.versionNumber} · {t(`submissions.statuses.${h.status}`)} · {formatDateTime(h.submittedAt, lang)}
              </article>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}
