import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { myProgress } from '../../api/deliveries.js'
import { ApiError } from '../../api/client.js'
import { statusStyle } from '../../utils/statusStyle.js'

export default function MyProgressPage() {
  const { t } = useTranslation()
  const [rows, setRows] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    myProgress()
      .then((data) => {
        if (!cancelled) {
          setRows(data)
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
      <p role="alert" className="alert alert-danger">
        {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
      </p>
    )
  }

  return (
    <div>
      <h1 className="page-title">{t('progress.myTitle')}</h1>
      <div tabIndex={0} role="region" aria-label={t('common.tableRegion')} className="mt-6 table-panel">
        <table className="w-full text-left text-sm">
          <thead className="bg-canvas text-muted">
            <tr>
              <th>{t('topics.name')}</th>
              <th>{t('progress.status')}</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.topicId} className="border-t border-line">
                <td className="font-medium">{row.topicName}</td>
                <td>
                  <span className={`badge ${statusStyle(row.effectiveStatus)}`}>
                    {t(`progress.statuses.${row.effectiveStatus}`)}
                    {row.manualStatus ? ` · ${t('progress.manual')}` : ''}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
