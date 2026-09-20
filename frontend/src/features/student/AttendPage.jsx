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
      <h1 className="page-title">{t('attend.title')}</h1>

      {state === 'sending' && <p role="status" className="state-message">{t('common.loading')}</p>}

      {state === 'done' && (
        <div className="mt-4 alert alert-success">
          <p className="text-lg font-bold text-success">{t('attend.success')}</p>
          {detail && <p className="mt-1 text-sm text-success">{detail.fullName} · {detail.controlNumber}</p>}
          <Link to="/app/sessions" className="mt-4 inline-block text-sm font-medium text-accent underline">
            {t('attend.backToSessions')}
          </Link>
        </div>
      )}

      {state === 'duplicate' && (
        <p role="alert" className="mt-4 alert alert-warning">
          {t('attend.duplicate')}
        </p>
      )}

      {state === 'expired' && (
        <p role="alert" className="mt-4 alert alert-warning">
          {t('attend.expired')}
        </p>
      )}

      {state === 'error' && (
        <p role="alert" className="mt-4 alert alert-danger">
          {t('errors.500')}
        </p>
      )}

      {(state === 'idle' || state === 'duplicate' || state === 'expired' || state === 'error') && (
        <form onSubmit={submitManual} className="mt-6 card">
          <label className="block text-left">
            <span className="mb-2 block text-sm font-medium text-ink">{t('attend.tokenLabel')}</span>
            <input
              value={manualToken}
              onChange={(e) => setManualToken(e.target.value)}
              placeholder={t('attend.tokenPlaceholder')}
              className="field font-mono"
            />
          </label>
          <button className="mt-3 w-full btn btn-primary">
            {t('attend.submit')}
          </button>
        </form>
      )}
    </div>
  )
}
