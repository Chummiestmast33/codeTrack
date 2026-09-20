import FormField from '../../components/FormField.jsx'
import { useCallback, useEffect, useState } from 'react'
import { useParams } from 'react-router'
import { useTranslation } from 'react-i18next'
import { getSession } from '../../api/sessions.js'
import { fetchQrImageUrl, getAttendance, getQr, regenerateQr, registerManualAttendance, updateAttendance } from '../../api/attendance.js'
import { api, ApiError } from '../../api/client.js'
import { formatDateTime } from '../../utils/format.js'
import { getLanguage } from '../../i18n.js'

const STATUSES = ['Present', 'Absent', 'Late', 'Excused']

export default function SessionDetailPage() {
  const { t, i18n } = useTranslation()
  const { id } = useParams()
  const lang = getLanguage().split('-')[0] || i18n.language
  const [session, setSession] = useState(null)
  const [rows, setRows] = useState([])
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)
  const [manual, setManual] = useState({ userId: '', status: 'Present', observation: '' })
  const [ticket, setTicket] = useState(null)
  const [qrUrl, setQrUrl] = useState(null)
  const [qrError, setQrError] = useState(null)

  const load = useCallback(async () => {
    const [s, att, list] = await Promise.all([
      getSession(id),
      getAttendance(id).catch((e) => {
        if (e instanceof ApiError && e.status === 404) return []
        throw e
      }),
      api('/api/admin/users'),
    ])
    setSession(s)
    setRows(att)
    setUsers(list.filter((u) => u.role === 'Student'))
  }, [id])

  useEffect(() => {
    let cancelled = false
    Promise.all([
      getSession(id),
      getAttendance(id).catch((e) => {
        if (e instanceof ApiError && e.status === 404) return []
        throw e
      }),
      api('/api/admin/users'),
    ])
      .then(([s, att, list]) => {
        if (!cancelled) {
          setSession(s)
          setRows(att)
          setUsers(list.filter((u) => u.role === 'Student'))
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
  }, [id])

  useEffect(() => () => {
    if (qrUrl) URL.revokeObjectURL(qrUrl)
  }, [qrUrl])

  async function refresh() {
    setError(null)
    try {
      await load()
    } catch (err) {
      setError(err)
    }
  }

  async function onManual(e) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await registerManualAttendance(id, {
        userId: manual.userId,
        status: manual.status,
        observation: manual.observation || null,
      })
      setManual({ userId: '', status: 'Present', observation: '' })
      await refresh()
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  async function onStatus(row, status) {
    setBusy(true)
    setError(null)
    try {
      await updateAttendance(row.id, { status, observation: row.observation })
      await refresh()
    } catch (err) {
      setError(err)
    } finally {
      setBusy(false)
    }
  }

  async function loadQr() {
    setQrError(null)
    try {
      const data = await getQr(id)
      setTicket(data)
      if (qrUrl) URL.revokeObjectURL(qrUrl)
      setQrUrl(await fetchQrImageUrl(id))
    } catch (err) {
      if (err instanceof ApiError && err.status === 410) {
        setTicket(null)
        setQrUrl(null)
        setQrError(t('qr.expired'))
      } else {
        setQrError(err instanceof ApiError ? t(`errors.${err.status}`, { defaultValue: t('errors.500') }) : t('errors.500'))
      }
    }
  }

  async function onRegenerate() {
    setBusy(true)
    setQrError(null)
    try {
      const data = await regenerateQr(id)
      setTicket(data)
      if (qrUrl) URL.revokeObjectURL(qrUrl)
      setQrUrl(await fetchQrImageUrl(id))
    } catch (err) {
      setQrError(err instanceof ApiError ? t(`errors.${err.status}`, { defaultValue: t('errors.500') }) : t('errors.500'))
    } finally {
      setBusy(false)
    }
  }

  function errorMessage() {
    if (!(error instanceof ApiError)) return error ? t('errors.500') : ''
    return t(`errors.${error.status}`, { defaultValue: t('errors.500') })
  }

  const input = 'field'

  if (loading) return <p role="status" className="state-message">{t('common.loading')}</p>
  if (error && !session) {
    return (
      <p role="alert" className="alert alert-danger">
        {errorMessage()}
      </p>
    )
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="page-title">{session.title}</h1>
        <p className="mt-1 text-muted">
          {formatDateTime(session.sessionDate, lang)} · {t(`sessions.statuses.${session.status}`, { defaultValue: session.status })}
        </p>
        <p className="text-sm text-muted">{session.topics.map((x) => x.name).join(', ')}</p>
      </div>

      {error && (
        <p role="alert" className="alert alert-danger">
          {errorMessage()}
        </p>
      )}

      <section className="card">
        <h2 className="font-bold">{t('qr.title')}</h2>
        {!ticket ? (
          <div className="mt-2 flex flex-wrap items-center gap-3">
            <button onClick={loadQr} className="btn btn-primary">
              {t('qr.show')}
            </button>
            <button disabled={busy} onClick={onRegenerate} className="btn">
              {t('qr.generate')}
            </button>
            {qrError && <span className="text-sm text-warning">{qrError}</span>}
          </div>
        ) : (
          <div className="mt-6 flex flex-wrap items-start gap-6">
            {qrUrl && <img src={qrUrl} alt={t('qr.title')} width={256} height={256} className="qr-image" />}
            <div className="min-w-0 flex-1 space-y-2 text-sm">
              <p className="break-all font-mono text-sm text-muted">{ticket.attendUrl}</p>
              <p className="text-muted">{t('qr.expires', { date: formatDateTime(ticket.expiresAt, lang) })}</p>
              <div className="flex flex-wrap gap-2">
                {qrUrl && (
                  <a href={qrUrl} download={`qr-${id}.png`} className="btn">
                    {t('qr.download')}
                  </a>
                )}
                <button disabled={busy} onClick={onRegenerate} className="btn">
                  {t('qr.regenerate')}
                </button>
              </div>
              {qrError && <p className="text-warning">{qrError}</p>}
            </div>
          </div>
        )}
      </section>

      <section className="card">
        <h2 className="font-bold">{t('attendance.manualTitle')}</h2>
        <form onSubmit={onManual} className="mt-6 grid max-w-xl gap-4">
          <FormField label={t('attendance.pickStudent')}><select aria-label={t('attendance.pickStudent')} value={manual.userId} onChange={(e) => setManual({ ...manual, userId: e.target.value })} className={input}>
            <option value="">{t('attendance.pickStudent')}</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.fullName} · {u.controlNumber}
              </option>
            ))}
          </select></FormField>
          <div className="grid gap-4 md:grid-cols-2">
            <FormField label={t('attendance.status')}><select aria-label={t('attendance.status')} value={manual.status} onChange={(e) => setManual({ ...manual, status: e.target.value })} className={input}>
              {STATUSES.map((s) => (
                <option key={s} value={s}>{t(`attendance.statuses.${s}`)}</option>
              ))}
            </select></FormField>
            <FormField label={t('attendance.observation')}><input aria-label={t('attendance.observation')} value={manual.observation} onChange={(e) => setManual({ ...manual, observation: e.target.value })} placeholder={t('attendance.observation')} className={input} /></FormField>
          </div>
          <button disabled={busy || !manual.userId} className="w-fit btn btn-primary">
            {t('common.save')}
          </button>
        </form>
      </section>

      <section>
        <h2 className="font-bold">{t('attendance.title', { count: rows.length })}</h2>
        {rows.length === 0 ? (
          <p role="status" className="state-message">{t('attendance.empty')}</p>
        ) : (
          <div tabIndex={0} role="region" aria-label={t('common.tableRegion')} className="mt-6 table-panel">
            <table className="w-full text-left text-sm">
              <thead className="bg-canvas text-muted">
                <tr>
                  <th>{t('auth.controlNumber')}</th>
                  <th>{t('auth.fullName')}</th>
                  <th>{t('attendance.status')}</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((row) => (
                  <tr key={row.id} className="border-t border-line">
                    <td className="font-mono">{row.controlNumber}</td>
                    <td>{row.fullName}</td>
                    <td>
                      <FormField label={t('attendance.status')}><select aria-label={t('attendance.status')}
                        value={row.status}
                        disabled={busy}
                        onChange={(e) => onStatus(row, e.target.value)}
                        className="field"
                      >
                        {STATUSES.map((s) => (
                          <option key={s} value={s}>{t(`attendance.statuses.${s}`)}</option>
                        ))}
                      </select></FormField>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}
