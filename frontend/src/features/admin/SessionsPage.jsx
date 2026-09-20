import FormField from '../../components/FormField.jsx'
import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { useTranslation } from 'react-i18next'
import { cancelSession, createSession, listSessions, markSessionImparted } from '../../api/sessions.js'
import { listTopics } from '../../api/topics.js'
import { ApiError } from '../../api/client.js'
import { formatDateTime } from '../../utils/format.js'
import { getLanguage } from '../../i18n.js'

export default function SessionsPage() {
  const { t, i18n } = useTranslation()
  const [sessions, setSessions] = useState([])
  const [topics, setTopics] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)
  const [form, setForm] = useState({ title: '', description: '', sessionDate: '', topicIds: [] })

  async function refresh() {
    setLoading(true)
    setError(null)
    try {
      const [s, top] = await Promise.all([listSessions(), listTopics()])
      setSessions(s)
      setTopics(top)
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    let cancelled = false
    Promise.all([listSessions(), listTopics()])
      .then(([s, top]) => {
        if (!cancelled) {
          setSessions(s)
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

  function toggleTopic(id) {
    setForm((f) => ({
      ...f,
      topicIds: f.topicIds.includes(id) ? f.topicIds.filter((x) => x !== id) : [...f.topicIds, id],
    }))
  }

  async function onCreate(e) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await createSession({
        title: form.title.trim(),
        description: form.description.trim() || null,
        sessionDate: new Date(form.sessionDate).toISOString(),
        topicIds: form.topicIds,
      })
      setForm({ title: '', description: '', sessionDate: '', topicIds: [] })
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
  const lang = getLanguage().split('-')[0] || i18n.language

  return (
    <div className="workspace-grid">
      <h1 className="page-title">{t('sessions.title')}</h1>

      {error && (
        <p role="alert" className="mt-4 alert alert-danger">
          {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
        </p>
      )}

      <form onSubmit={onCreate} className="grid gap-4 card workspace-editor">
        <h2 className="font-bold">{t('sessions.create')}</h2>
        <FormField label={t('sessions.sessionTitle')}><input aria-label={t('sessions.sessionTitle')} value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} placeholder={t('sessions.sessionTitle')} className={input} /></FormField>
        <FormField label={t('sessions.description')}><input aria-label={t('sessions.description')} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder={t('sessions.description')} className={input} /></FormField>
        <FormField label={t('sessions.date')}><input aria-label={t('sessions.date')} type="datetime-local" value={form.sessionDate} onChange={(e) => setForm({ ...form, sessionDate: e.target.value })} className={input} /></FormField>
        <fieldset>
          <legend className="text-sm font-medium text-ink">{t('sessions.topics')}</legend>
          <div className="mt-1 flex flex-wrap gap-2">
            {topics.filter((x) => x.isActive).map((topic) => (
              <label key={topic.id} className="topic-choice">
                <input type="checkbox" checked={form.topicIds.includes(topic.id)} onChange={() => toggleTopic(topic.id)} />
                {topic.name}
              </label>
            ))}
          </div>
        </fieldset>
        <button disabled={busy} className="w-fit btn btn-primary">
          {t('common.save')}
        </button>
      </form>

      {loading ? (
        <p role="status" className="state-message">{t('common.loading')}</p>
      ) : (
        <div tabIndex={0} role="region" aria-label={t('common.tableRegion')} className="table-panel">
          <table className="w-full text-left text-sm">
            <thead className="bg-canvas text-muted">
              <tr>
                <th>{t('sessions.sessionTitle')}</th>
                <th>{t('sessions.date')}</th>
                <th>{t('sessions.topics')}</th>
                <th>{t('sessions.status')}</th>
                <th>{t('admin.users.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {sessions.map((s) => (
                <tr key={s.id} className="border-t border-line">
                  <td className="font-medium">
                    <Link to={`/admin/sessions/${s.id}`} className="text-accent underline">{s.title}</Link>
                  </td>
                  <td>{formatDateTime(s.sessionDate, lang)}</td>
                  <td>{s.topics.map((x) => x.name).join(', ')}</td>
                  <td>{t(`sessions.statuses.${s.status}`, { defaultValue: s.status })}</td>
                  <td>
                    <div className="flex flex-wrap gap-2">
                      {s.status === 'Planned' && (
                        <>
                          <button disabled={busy} onClick={() => run(s.id, markSessionImparted)} className="btn btn-success">
                            {t('sessions.markImparted')}
                          </button>
                          <button disabled={busy} onClick={() => run(s.id, cancelSession)} className="btn btn-danger">
                            {t('sessions.cancel')}
                          </button>
                        </>
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
