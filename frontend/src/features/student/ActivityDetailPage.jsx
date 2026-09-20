import FormField from '../../components/FormField.jsx'
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

  const input = 'field'
  if (loading) return <p role="status" className="state-message">{t('common.loading')}</p>
  if (error && !activity) {
    return (
      <p role="alert" className="alert alert-danger">
        {errorMessage()}
      </p>
    )
  }

  const needsUrl = activity.submissionMode === 'UrlOnly' || activity.submissionMode === 'UrlAndFile'
  const needsFile = activity.submissionMode === 'FileOnly' || activity.submissionMode === 'UrlAndFile'
  const allowsFile = needsFile || activity.submissionMode === 'UrlOrFile'
  const current = history.length > 0 ? history[history.length - 1] : null

  return (
    <div className="space-y-8">
      <div>
        <h1 className="page-title">{activity.title}</h1>
        <p className="text-sm text-muted">
          {activity.topicName}
          {activity.dueDate ? ` · ${t('activities.due')}: ${formatDate(activity.dueDate, lang)}` : ''}
          {' · '}{t(`activities.modes.${activity.submissionMode}`)}
        </p>
      </div>

      <section className="card">
        <SafeMarkdown text={activity.markdownContent} />
      </section>

      {error && (
        <p role="alert" className="alert alert-danger">
          {errorMessage()}
        </p>
      )}
      {notice && (
        <p role="status" className="alert alert-success">
          {notice}
        </p>
      )}

      <section className="card">
        <h2 className="font-bold">{t('submissions.deliver')}</h2>
        <form onSubmit={onSubmit} className="mt-6 grid max-w-xl gap-4">
          {(needsUrl || activity.submissionMode === 'UrlOrFile') && (
            <FormField label={t('submissions.url')}><input aria-label={t('submissions.url')} value={form.url} onChange={(e) => setForm({ ...form, url: e.target.value })} placeholder="https://…" className={input} /></FormField>
          )}
          {allowsFile && (
            <FormField label={t('submissions.file')}><input aria-label={t('submissions.file')} type="file" onChange={(e) => setForm({ ...form, file: e.target.files?.[0] ?? null })} className={input} /></FormField>
          )}
          <FormField label={t('submissions.commentPlaceholder')}><input aria-label={t('submissions.commentPlaceholder')} value={form.comment} onChange={(e) => setForm({ ...form, comment: e.target.value })} placeholder={t('submissions.commentPlaceholder')} className={input} /></FormField>
          <button disabled={busy} className="w-fit btn btn-primary">
            {busy ? t('common.loading') : t('submissions.deliver')}
          </button>
        </form>
        <p className="mt-2 text-sm text-muted">{t('submissions.deliveryHint')}</p>
      </section>

      <section>
        <h2 className="font-bold">{t('submissions.history', { count: history.length })}</h2>
        {history.length === 0 ? (
          <p role="status" className="state-message">{t('submissions.noHistory')}</p>
        ) : (
          <div className="mt-6 space-y-6">
            {current && (
              <article className="card card-accent text-sm">
                <p className="font-bold">{t('submissions.current')} · v{current.versionNumber} · {t(`submissions.statuses.${current.status}`)}{current.isLate ? ` · ${t('submissions.late')}` : ''}</p>
                {current.url && <a href={current.url} target="_blank" rel="noreferrer" className="break-all text-accent underline">{current.url}</a>}
                {current.fileName && <p className="text-muted">{current.fileName}</p>}
                {current.instructorComment && <p className="mt-1 text-ink">{t('submissions.instructorComment')}: {current.instructorComment}</p>}
                <p className="mt-1 text-sm text-muted">{formatDateTime(current.submittedAt, lang)}</p>
              </article>
            )}
            {history.slice(0, -1).reverse().map((h) => (
              <article key={h.id} className="card text-sm text-muted">
                v{h.versionNumber} · {t(`submissions.statuses.${h.status}`)} · {formatDateTime(h.submittedAt, lang)}
              </article>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}
