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

  if (loading) return <p role="status" className="state-message">{t('common.loading')}</p>
  if (error) {
    return (
      <p role="alert" className="mt-4 alert alert-danger">
        {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
      </p>
    )
  }

  return (
    <div>
      <h1 className="page-title">{t('activities.title')}</h1>
      {activities.length === 0 ? (
        <p role="status" className="state-message">{t('activities.empty')}</p>
      ) : (
        <ul className="mt-6 space-y-4">
          {activities.map((a) => (
            <li key={a.id} className="card">
              <Link to={`/app/activities/${a.id}`} className="font-bold text-accent underline">{a.title}</Link>
              <p className="text-sm text-muted">
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
