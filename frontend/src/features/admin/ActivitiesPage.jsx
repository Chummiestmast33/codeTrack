import FormField from '../../components/FormField.jsx'
import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { useTranslation } from 'react-i18next'
import { closeActivity, createActivity, listActivities, publishActivity } from '../../api/activities.js'
import { listTopics } from '../../api/topics.js'
import { ApiError } from '../../api/client.js'

const MODES = ['UrlOnly', 'FileOnly', 'UrlOrFile', 'UrlAndFile']

export default function ActivitiesPage() {
  const { t } = useTranslation()
  const [activities, setActivities] = useState([])
  const [topics, setTopics] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)
  const [form, setForm] = useState({ title: '', markdownContent: '', topicId: '', dueDate: '', submissionMode: 'UrlOrFile' })

  async function refresh() {
    setLoading(true)
    setError(null)
    try {
      const [a, top] = await Promise.all([listActivities(), listTopics()])
      setActivities(a)
      setTopics(top)
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    let cancelled = false
    Promise.all([listActivities(), listTopics()])
      .then(([a, top]) => {
        if (!cancelled) {
          setActivities(a)
          setTopics(top)
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
  }, [])

  async function onCreate(e) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await createActivity({
        title: form.title.trim(),
        markdownContent: form.markdownContent,
        topicId: form.topicId,
        sessionId: null,
        dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null,
        submissionMode: form.submissionMode,
      })
      setForm({ title: '', markdownContent: '', topicId: '', dueDate: '', submissionMode: 'UrlOrFile' })
      await refresh()
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  async function run(id, action) {
    setBusy(true)
    setError(null)
    try {
      await action(id)
      await refresh()
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  const input = 'field'

  return (
    <div className="workspace-grid">
      <h1 className="page-title">{t('activities.title')}</h1>

      {error && (
        <p role="alert" className="mt-4 alert alert-danger">
          {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
        </p>
      )}

      <form onSubmit={onCreate} className="grid gap-4 card workspace-editor">
        <h2 className="font-bold">{t('activities.create')}</h2>
        <FormField label={t('activities.activityTitle')}><input aria-label={t('activities.activityTitle')} value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} placeholder={t('activities.activityTitle')} className={input} /></FormField>
        <FormField label={t('activities.pickTopic')}><select aria-label={t('activities.pickTopic')} value={form.topicId} onChange={(e) => setForm({ ...form, topicId: e.target.value })} className={input}>
          <option value="">{t('activities.pickTopic')}</option>
          {topics.filter((x) => x.isActive).map((topic) => (
            <option key={topic.id} value={topic.id}>{topic.name}</option>
          ))}
        </select></FormField>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField label={t('activities.due')}><input aria-label={t('activities.due')} type="datetime-local" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} className={input} /></FormField>
          <FormField label={t('activities.submissionMode')}><select aria-label={t('activities.submissionMode')} value={form.submissionMode} onChange={(e) => setForm({ ...form, submissionMode: e.target.value })} className={input}>
            {MODES.map((m) => (
              <option key={m} value={m}>{t(`activities.modes.${m}`)}</option>
            ))}
          </select></FormField>
        </div>
        <FormField label={t('activities.markdownHint')}><textarea aria-label={t('activities.markdownHint')} value={form.markdownContent} onChange={(e) => setForm({ ...form, markdownContent: e.target.value })} rows={4} placeholder={t('activities.markdownHint')} className={`${input} font-mono`} /></FormField>
        <button disabled={busy} className="w-fit btn btn-primary">
          {t('common.save')}
        </button>
      </form>

      {loading ? (
        <p role="status" className="state-message">{t('common.loading')}</p>
      ) : activities.length === 0 ? (
        <p role="status" className="state-message">{t('activities.empty')}</p>
      ) : (
        <div tabIndex={0} role="region" aria-label={t('common.tableRegion')} className="table-panel">
          <table className="w-full text-left text-sm">
            <thead className="bg-canvas text-muted">
              <tr>
                <th>{t('activities.activityTitle')}</th>
                <th>{t('activities.topic')}</th>
                <th>{t('activities.status')}</th>
                <th>{t('admin.users.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {activities.map((a) => (
                <tr key={a.id} className="border-t border-line">
                  <td>
                    <Link to={`/admin/activities/${a.id}/submissions`} className="font-medium text-accent underline">{a.title}</Link>
                    <span className="ml-2 text-sm text-muted">{t(`activities.modes.${a.submissionMode}`)}</span>
                  </td>
                  <td>{a.topicName}</td>
                  <td>{t(`activities.statuses.${a.status}`, { defaultValue: a.status })}</td>
                  <td>
                    <div className="flex flex-wrap gap-2">
                      {a.status === 'Draft' && (
                        <button disabled={busy} onClick={() => run(a.id, publishActivity)} className="btn btn-success">
                          {t('activities.publish')}
                        </button>
                      )}
                      {a.status !== 'Closed' && (
                        <button disabled={busy} onClick={() => run(a.id, closeActivity)} className="btn btn-danger">
                          {t('activities.close')}
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
