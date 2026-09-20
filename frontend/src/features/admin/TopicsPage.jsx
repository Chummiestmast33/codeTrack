import FormField from '../../components/FormField.jsx'
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

  const input = 'field'

  return (
    <div className="workspace-grid">
      <h1 className="page-title">{t('topics.title')}</h1>

      {error && (
        <p role="alert" className="mt-4 alert alert-danger">
          {errorMessage()}
        </p>
      )}

      <form onSubmit={onCreate} className="grid gap-4 card workspace-editor">
        <h2 className="font-bold">{t('topics.create')}</h2>
        <FormField label={t('topics.name')}><input aria-label={t('topics.name')} value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder={t('topics.name')} className={input} /></FormField>
        <FormField label={t('topics.description')}><input aria-label={t('topics.description')} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder={t('topics.description')} className={input} /></FormField>
        <FormField label={t('topics.order')}><input aria-label={t('topics.order')} type="number" min="0" value={form.orderNumber} onChange={(e) => setForm({ ...form, orderNumber: e.target.value })} placeholder={t('topics.order')} className={input} /></FormField>
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
                <th>{t('topics.order')}</th>
                <th>{t('topics.name')}</th>
                <th>{t('topics.status')}</th>
                <th>{t('admin.users.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {topics.map((topic) => (
                <tr key={topic.id} className="border-t border-line">
                  <td>{topic.orderNumber}</td>
                  <td>
                    {editing?.id === topic.id ? (
                      <form onSubmit={onUpdate} className="flex flex-wrap gap-2">
                        <FormField label={t('topics.name')}><input aria-label={t('topics.name')} value={editing.name} onChange={(e) => setEditing({ ...editing, name: e.target.value })} className={input} /></FormField>
                        <FormField label={t('submissions.statusLabel')}><input aria-label={t('submissions.statusLabel')} value={editing.description ?? ''} onChange={(e) => setEditing({ ...editing, description: e.target.value })} className={input} /></FormField>
                        <FormField label={t('topics.order')}><input aria-label={t('topics.order')} type="number" min="0" value={editing.orderNumber} onChange={(e) => setEditing({ ...editing, orderNumber: e.target.value })} className="w-20 field" /></FormField>
                        <button className="btn btn-primary">{t('common.save')}</button>
                        <button type="button" onClick={() => setEditing(null)} className="btn">{t('common.cancel')}</button>
                      </form>
                    ) : (
                      <span className="font-medium">{topic.name}</span>
                    )}
                  </td>
                  <td>
                    <span className={`badge ${topic.isActive ? 'bg-success-soft text-success' : 'bg-raised text-muted'}`}>
                      {topic.isActive ? t('topics.active') : t('topics.inactive')}
                    </span>
                  </td>
                  <td>
                    <div className="flex flex-wrap gap-2">
                      <button disabled={busy} onClick={() => setEditing({ id: topic.id, name: topic.name, description: topic.description, orderNumber: topic.orderNumber })} className="btn">
                        {t('topics.edit')}
                      </button>
                      <button disabled={busy} onClick={() => toggle(topic)} className="btn">
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
