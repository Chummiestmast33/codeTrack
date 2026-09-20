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

  if (loading) return <p className="text-slate-600">{t('common.loading')}</p>
  if (error) {
    return (
      <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
        {error instanceof ApiError ? t(`errors.${error.status}`, { defaultValue: t('errors.500') }) : t('errors.500')}
      </p>
    )
  }

  return (
    <div>
      <h1 className="text-2xl font-bold">{t('progress.myTitle')}</h1>
      <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white shadow-sm">
        <table className="w-full text-left text-sm">
          <thead className="bg-slate-50 text-slate-500">
            <tr>
              <th className="px-4 py-2">{t('topics.name')}</th>
              <th className="px-4 py-2">{t('progress.status')}</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.topicId} className="border-t border-slate-100">
                <td className="px-4 py-2 font-medium">{row.topicName}</td>
                <td className="px-4 py-2">
                  <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${statusStyle(row.effectiveStatus)}`}>
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
