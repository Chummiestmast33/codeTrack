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
      <h1 className="text-2xl font-bold">{t('sessions.title')}</h1>
      {sessions.length === 0 ? (
        <p className="mt-4 text-slate-600">{t('sessions.empty')}</p>
      ) : (
        <ul className="mt-4 space-y-3">
          {sessions.map((s) => (
            <li key={s.id} className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
              <p className="font-bold">{s.title}</p>
              <p className="text-sm text-slate-600">
                {formatDateTime(s.sessionDate, lang)} · {t(`sessions.statuses.${s.status}`, { defaultValue: s.status })}
              </p>
              <p className="text-sm text-slate-500">{s.topics.map((x) => x.name).join(', ')}</p>
              {s.status === 'Planned' && (
                <Link to="/app/attend" className="mt-2 inline-block text-sm font-medium text-indigo-600 underline">
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
