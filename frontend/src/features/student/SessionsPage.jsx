import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { useTranslation } from 'react-i18next'
import { listSessions } from '../../api/sessions.js'
import { ApiError } from '../../api/client.js'
import { formatDateTime } from '../../utils/format.js'
import { getLanguage } from '../../i18n.js'

export default function StudentSessionsPage() {
  const { t, i18n } = useTranslation()
  const lang = getLanguage().split('-')[0] || i18n.language
  const [sessions, setSessions] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    listSessions()
      .then((data) => {
        if (!cancelled) {
          setSessions(data)
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
      <h1 className="page-title">{t('sessions.title')}</h1>
      {sessions.length === 0 ? (
        <p role="status" className="state-message">{t('sessions.empty')}</p>
      ) : (
        <ul className="mt-6 space-y-4">
          {sessions.map((s) => (
            <li key={s.id} className="card">
              <p className="font-bold">{s.title}</p>
              <p className="text-sm text-muted">
                {formatDateTime(s.sessionDate, lang)} · {t(`sessions.statuses.${s.status}`, { defaultValue: s.status })}
              </p>
              <p className="text-sm text-muted">{s.topics.map((x) => x.name).join(', ')}</p>
              {s.status === 'Planned' && (
                <Link to="/app/attend" className="mt-2 inline-block text-sm font-medium text-accent underline">
                  {t('attend.registerCta')}
                </Link>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
