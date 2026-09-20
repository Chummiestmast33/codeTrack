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

  const input = 'w-full rounded-md border border-slate-300 px-3 py-2'
  const lang = getLanguage().split('-')[0] || i18n.language

  return (
    <div>
      <h1 className="text-2xl font-bold">{t('sessions.title')}</h1>

      {error && (
        <p role="alert" className="mt-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
          {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
        </p>
      )}

      <form onSubmit={onCreate} className="mt-4 grid max-w-2xl gap-3 rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <h2 className="font-bold">{t('sessions.create')}</h2>
        <input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} placeholder={t('sessions.sessionTitle')} className={input} />
        <input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder={t('sessions.description')} className={input} />
        <input type="datetime-local" value={form.sessionDate} onChange={(e) => setForm({ ...form, sessionDate: e.target.value })} className={input} />
        <fieldset>
          <legend className="text-sm font-medium text-slate-700">{t('sessions.topics')}</legend>
          <div className="mt-1 flex flex-wrap gap-2">
            {topics.filter((x) => x.isActive).map((topic) => (
              <label key={topic.id} className="inline-flex items-center gap-1.5 rounded-full border border-slate-300 px-3 py-1 text-sm">
                <input type="checkbox" checked={form.topicIds.includes(topic.id)} onChange={() => toggleTopic(topic.id)} />
                {topic.name}
              </label>
            ))}
          </div>
        </fieldset>
        <button disabled={busy} className="w-fit rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50">
          {t('common.save')}
        </button>
      </form>

      {loading ? (
        <p className="mt-6 text-slate-600">{t('common.loading')}</p>
      ) : (
        <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white shadow-sm">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 text-slate-500">
              <tr>
                <th className="px-4 py-2">{t('sessions.sessionTitle')}</th>
                <th className="px-4 py-2">{t('sessions.date')}</th>
                <th className="px-4 py-2">{t('sessions.topics')}</th>
                <th className="px-4 py-2">{t('sessions.status')}</th>
                <th className="px-4 py-2">{t('admin.users.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {sessions.map((s) => (
                <tr key={s.id} className="border-t border-slate-100">
                  <td className="px-4 py-2 font-medium">
                    <Link to={`/admin/sessions/${s.id}`} className="text-indigo-600 underline">{s.title}</Link>
                  </td>
                  <td className="px-4 py-2">{formatDateTime(s.sessionDate, lang)}</td>
                  <td className="px-4 py-2">{s.topics.map((x) => x.name).join(', ')}</td>
                  <td className="px-4 py-2">{t(`sessions.statuses.${s.status}`, { defaultValue: s.status })}</td>
                  <td className="px-4 py-2">
                    <div className="flex gap-1.5">
                      {s.status === 'Planned' && (
                        <>
                          <button disabled={busy} onClick={() => run(s.id, markSessionImparted)} className="rounded-md border border-green-300 px-2 py-1 text-xs text-green-700 hover:bg-green-50">
                            {t('sessions.markImparted')}
                          </button>
                          <button disabled={busy} onClick={() => run(s.id, cancelSession)} className="rounded-md border border-red-300 px-2 py-1 text-xs text-red-700 hover:bg-red-50">
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
