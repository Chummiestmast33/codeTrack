import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { useTranslation } from 'react-i18next'
import { listActivities } from '../../api/activities.js'
import { ApiError } from '../../api/client.js'
import { formatDate } from '../../utils/format.js'
import { getLanguage } from '../../i18n.js'

export default function StudentActivitiesPage() {
  const { t, i18n } = useTranslation()
  const lang = getLanguage().split('-')[0] || i18n.language
  const [activities, setActivities] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    listActivities()
      .then((data) => {
        if (!cancelled) {
          setActivities(data.filter((a) => a.status === 'Published'))
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

  if (loading) return <p className="text-slate-600">{t('common.loading')}</p>
  if (error) {
    return (
      <p role="alert" className="mt-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
        {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
      </p>
    )
  }

  return (
    <div>
      <h1 className="text-2xl font-bold">{t('activities.title')}</h1>
      {activities.length === 0 ? (
        <p className="mt-4 text-slate-600">{t('activities.empty')}</p>
      ) : (
        <ul className="mt-4 space-y-3">
          {activities.map((a) => (
            <li key={a.id} className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
              <Link to={`/app/activities/${a.id}`} className="font-bold text-indigo-600 underline">{a.title}</Link>
              <p className="text-sm text-slate-500">
                {a.topicName}
                {a.dueDate ? ` · ${t('activities.due')}: ${formatDate(a.dueDate, lang)}` : ''}
              </p>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
