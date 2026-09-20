import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { activateTopic, createTopic, deactivateTopic, listTopics, updateTopic } from '../../api/topics.js'
import { ApiError } from '../../api/client.js'

export default function TopicsPage() {
  const { t } = useTranslation()
  const [topics, setTopics] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [form, setForm] = useState({ name: '', description: '', orderNumber: 0 })
  const [editing, setEditing] = useState(null)
  const [busy, setBusy] = useState(false)

  async function refresh() {
    setLoading(true)
    setError(null)
    try {
      setTopics(await listTopics())
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    let cancelled = false
    listTopics()
      .then((data) => {
        if (!cancelled) {
          setTopics(data)
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
      await createTopic({ ...form, orderNumber: Number(form.orderNumber) })
      setForm({ name: '', description: '', orderNumber: 0 })
      await refresh()
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  async function onUpdate(e) {
    e.preventDefault()
    if (!editing) return
    setBusy(true)
    setError(null)
    try {
      await updateTopic(editing.id, {
        name: editing.name,
        description: editing.description,
        orderNumber: Number(editing.orderNumber),
      })
      setEditing(null)
      await refresh()
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  async function toggle(topic) {
    setBusy(true)
    setError(null)
    try {
      if (topic.isActive) await deactivateTopic(topic.id)
      else await activateTopic(topic.id)
      await refresh()
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  function errorMessage() {
    if (!(error instanceof ApiError)) return error ? t('errors.500') : ''
    return t(`errors.${error.status}`, { defaultValue: t('errors.500') })
  }

  const input = 'w-full rounded-md border border-slate-300 px-3 py-2'

  return (
    <div>
      <h1 className="text-2xl font-bold">{t('topics.title')}</h1>

      {error && (
        <p role="alert" className="mt-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
          {errorMessage()}
        </p>
      )}

      <form onSubmit={onCreate} className="mt-4 grid max-w-2xl gap-3 rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <h2 className="font-bold">{t('topics.create')}</h2>
        <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder={t('topics.name')} className={input} />
        <input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder={t('topics.description')} className={input} />
        <input type="number" min="0" value={form.orderNumber} onChange={(e) => setForm({ ...form, orderNumber: e.target.value })} placeholder={t('topics.order')} className={input} />
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
                <th className="px-4 py-2">{t('topics.order')}</th>
                <th className="px-4 py-2">{t('topics.name')}</th>
                <th className="px-4 py-2">{t('topics.status')}</th>
                <th className="px-4 py-2">{t('admin.users.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {topics.map((topic) => (
                <tr key={topic.id} className="border-t border-slate-100">
                  <td className="px-4 py-2">{topic.orderNumber}</td>
                  <td className="px-4 py-2">
                    {editing?.id === topic.id ? (
                      <form onSubmit={onUpdate} className="flex flex-wrap gap-2">
                        <input value={editing.name} onChange={(e) => setEditing({ ...editing, name: e.target.value })} className={input} />
                        <input value={editing.description ?? ''} onChange={(e) => setEditing({ ...editing, description: e.target.value })} className={input} />
                        <input type="number" min="0" value={editing.orderNumber} onChange={(e) => setEditing({ ...editing, orderNumber: e.target.value })} className="w-20 rounded-md border border-slate-300 px-2 py-2" />
                        <button className="rounded-md bg-indigo-600 px-3 py-1.5 text-xs font-medium text-white">{t('common.save')}</button>
                        <button type="button" onClick={() => setEditing(null)} className="rounded-md border border-slate-300 px-3 py-1.5 text-xs">{t('common.cancel')}</button>
                      </form>
                    ) : (
                      <span className="font-medium">{topic.name}</span>
                    )}
                  </td>
                  <td className="px-4 py-2">
                    <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${topic.isActive ? 'bg-green-100 text-green-800' : 'bg-slate-200 text-slate-600'}`}>
                      {topic.isActive ? t('topics.active') : t('topics.inactive')}
                    </span>
                  </td>
                  <td className="px-4 py-2">
                    <div className="flex gap-1.5">
                      <button disabled={busy} onClick={() => setEditing({ id: topic.id, name: topic.name, description: topic.description, orderNumber: topic.orderNumber })} className="rounded-md border border-slate-300 px-2 py-1 text-xs hover:bg-slate-100">
                        {t('topics.edit')}
                      </button>
                      <button disabled={busy} onClick={() => toggle(topic)} className="rounded-md border border-slate-300 px-2 py-1 text-xs hover:bg-slate-100">
                        {topic.isActive ? t('topics.deactivate') : t('topics.activate')}
                      </button>
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
