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

  const input = 'w-full rounded-md border border-slate-300 px-3 py-2'

  return (
    <div>
      <h1 className="text-2xl font-bold">{t('activities.title')}</h1>

      {error && (
        <p role="alert" className="mt-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
          {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
        </p>
      )}

      <form onSubmit={onCreate} className="mt-4 grid max-w-2xl gap-3 rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <h2 className="font-bold">{t('activities.create')}</h2>
        <input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} placeholder={t('activities.activityTitle')} className={input} />
        <select value={form.topicId} onChange={(e) => setForm({ ...form, topicId: e.target.value })} className={input}>
          <option value="">{t('activities.pickTopic')}</option>
          {topics.filter((x) => x.isActive).map((topic) => (
            <option key={topic.id} value={topic.id}>{topic.name}</option>
          ))}
        </select>
        <div className="grid gap-3 sm:grid-cols-2">
          <input type="datetime-local" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} className={input} />
          <select value={form.submissionMode} onChange={(e) => setForm({ ...form, submissionMode: e.target.value })} className={input}>
            {MODES.map((m) => (
              <option key={m} value={m}>{t(`activities.modes.${m}`)}</option>
            ))}
          </select>
        </div>
        <textarea value={form.markdownContent} onChange={(e) => setForm({ ...form, markdownContent: e.target.value })} rows={4} placeholder={t('activities.markdownHint')} className={`${input} font-mono`} />
        <button disabled={busy} className="w-fit rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50">
          {t('common.save')}
        </button>
      </form>

      {loading ? (
        <p className="mt-6 text-slate-600">{t('common.loading')}</p>
      ) : activities.length === 0 ? (
        <p className="mt-6 text-slate-600">{t('activities.empty')}</p>
      ) : (
        <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white shadow-sm">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 text-slate-500">
              <tr>
                <th className="px-4 py-2">{t('activities.activityTitle')}</th>
                <th className="px-4 py-2">{t('activities.topic')}</th>
                <th className="px-4 py-2">{t('activities.status')}</th>
                <th className="px-4 py-2">{t('admin.users.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {activities.map((a) => (
                <tr key={a.id} className="border-t border-slate-100">
                  <td className="px-4 py-2">
                    <Link to={`/admin/activities/${a.id}/submissions`} className="font-medium text-indigo-600 underline">{a.title}</Link>
                    <span className="ml-2 text-xs text-slate-400">{t(`activities.modes.${a.submissionMode}`)}</span>
                  </td>
                  <td className="px-4 py-2">{a.topicName}</td>
                  <td className="px-4 py-2">{t(`activities.statuses.${a.status}`, { defaultValue: a.status })}</td>
                  <td className="px-4 py-2">
                    <div className="flex gap-1.5">
                      {a.status === 'Draft' && (
                        <button disabled={busy} onClick={() => run(a.id, publishActivity)} className="rounded-md border border-green-300 px-2 py-1 text-xs text-green-700 hover:bg-green-50">
                          {t('activities.publish')}
                        </button>
                      )}
                      {a.status !== 'Closed' && (
                        <button disabled={busy} onClick={() => run(a.id, closeActivity)} className="rounded-md border border-red-300 px-2 py-1 text-xs text-red-700 hover:bg-red-50">
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
