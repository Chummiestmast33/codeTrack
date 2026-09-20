import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { useTranslation } from 'react-i18next'
import { registerByQr } from '../../api/attendance.js'
import { ApiError } from '../../api/client.js'

export default function AttendPage() {
  const { t } = useTranslation()
  const { token } = useParams()
  const navigate = useNavigate()
  const [manualToken, setManualToken] = useState('')
  const [state, setState] = useState(token ? 'sending' : 'idle')
  const [detail, setDetail] = useState(null)

  useEffect(() => {
    if (!token) return
    let cancelled = false
    registerByQr(token)
      .then((data) => {
        if (!cancelled) {
          setDetail(data)
          setState('done')
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setState(err instanceof ApiError && err.status === 409 ? 'duplicate' : err instanceof ApiError && err.status === 410 ? 'expired' : 'error')
        }
      })
    return () => {
      cancelled = true
    }
  }, [token])

  function submitManual(e) {
    e.preventDefault()
    if (manualToken.trim()) navigate(`/app/attend/${manualToken.trim()}`)
  }

  return (
    <div className="mx-auto max-w-md text-center">
      <h1 className="text-2xl font-bold">{t('attend.title')}</h1>

      {state === 'sending' && <p className="mt-4 text-slate-600">{t('common.loading')}</p>}

      {state === 'done' && (
        <div className="mt-4 rounded-lg border border-green-200 bg-green-50 p-6">
          <p className="text-lg font-bold text-green-800">{t('attend.success')}</p>
          {detail && <p className="mt-1 text-sm text-green-700">{detail.fullName} · {detail.controlNumber}</p>}
          <Link to="/app/sessions" className="mt-4 inline-block text-sm font-medium text-indigo-600 underline">
            {t('attend.backToSessions')}
          </Link>
        </div>
      )}

      {state === 'duplicate' && (
        <p role="alert" className="mt-4 rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-800">
          {t('attend.duplicate')}
        </p>
      )}

      {state === 'expired' && (
        <p role="alert" className="mt-4 rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-800">
          {t('attend.expired')}
        </p>
      )}

      {state === 'error' && (
        <p role="alert" className="mt-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
          {t('errors.500')}
        </p>
      )}

      {(state === 'idle' || state === 'duplicate' || state === 'expired' || state === 'error') && (
        <form onSubmit={submitManual} className="mt-6 rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
          <label className="block text-left">
            <span className="mb-1 block text-sm font-medium text-slate-700">{t('attend.tokenLabel')}</span>
            <input
              value={manualToken}
              onChange={(e) => setManualToken(e.target.value)}
              placeholder={t('attend.tokenPlaceholder')}
              className="w-full rounded-md border border-slate-300 px-3 py-2 font-mono"
            />
          </label>
          <button className="mt-3 w-full rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500">
            {t('attend.submit')}
          </button>
        </form>
      )}
    </div>
  )
}
