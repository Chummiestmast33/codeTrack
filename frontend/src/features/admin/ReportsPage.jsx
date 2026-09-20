import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { downloadReport } from '../../api/deliveries.js'

const TARGETS = [
  { kind: 'attendance', formats: ['pdf', 'csv'] },
  { kind: 'progress', formats: ['pdf', 'csv'] },
]

export default function ReportsPage() {
  const { t } = useTranslation()
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(null)

  async function download(kind, format) {
    const key = `${kind}-${format}`
    setBusy(key)
    setError(null)
    try {
      const { url, filename } = await downloadReport(kind, format)
      const a = document.createElement('a')
      a.href = url
      a.download = filename
      document.body.appendChild(a)
      a.click()
      a.remove()
      setTimeout(() => URL.revokeObjectURL(url), 5000)
    } catch {
      setError(t('errors.500'))
    } finally {
      setBusy(null)
    }
  }

  return (
    <div>
      <h1 className="text-2xl font-bold">{t('reports.title')}</h1>
      <p className="mt-1 text-sm text-slate-600">{t('reports.description')}</p>

      {error && (
        <p role="alert" className="mt-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
          {error}
        </p>
      )}

      <div className="mt-4 grid max-w-2xl gap-4">
        {TARGETS.map(({ kind, formats }) => (
          <section key={kind} className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
            <h2 className="font-bold">{t(`reports.${kind}`)}</h2>
            <div className="mt-3 flex gap-2">
              {formats.map((format) => (
                <button
                  key={format}
                  disabled={busy !== null}
                  onClick={() => download(kind, format)}
                  className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
                >
                  {busy === `${kind}-${format}` ? t('common.loading') : t('reports.download', { format: format.toUpperCase() })}
                </button>
              ))}
            </div>
          </section>
        ))}
      </div>
    </div>
  )
}
