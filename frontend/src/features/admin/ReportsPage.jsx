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
      <h1 className="page-title">{t('reports.title')}</h1>
      <p className="mt-1 text-sm text-muted">{t('reports.description')}</p>

      {error && (
        <p role="alert" className="mt-4 alert alert-danger">
          {error}
        </p>
      )}

      <div className="mt-6 grid max-w-2xl gap-4">
        {TARGETS.map(({ kind, formats }) => (
          <section key={kind} className="card">
            <h2 className="font-bold">{t(`reports.${kind}`)}</h2>
            <div className="mt-3 flex flex-wrap gap-2">
              {formats.map((format) => (
                <button
                  key={format}
                  disabled={busy !== null}
                  onClick={() => download(kind, format)}
                  className="btn btn-primary"
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
