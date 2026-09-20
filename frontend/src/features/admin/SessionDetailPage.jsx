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

  const input = 'w-full rounded-md border border-slate-300 px-3 py-2'

  if (loading) return <p className="text-slate-600">{t('common.loading')}</p>
  if (error && !session) {
    return (
      <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
        {errorMessage()}
      </p>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">{session.title}</h1>
        <p className="mt-1 text-slate-600">
          {formatDateTime(session.sessionDate, lang)} · {t(`sessions.statuses.${session.status}`, { defaultValue: session.status })}
        </p>
        <p className="text-sm text-slate-500">{session.topics.map((x) => x.name).join(', ')}</p>
      </div>

      {error && (
        <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
          {errorMessage()}
        </p>
      )}

      <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <h2 className="font-bold">{t('qr.title')}</h2>
        {!ticket ? (
          <div className="mt-2 flex flex-wrap items-center gap-3">
            <button onClick={loadQr} className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500">
              {t('qr.show')}
            </button>
            <button disabled={busy} onClick={onRegenerate} className="rounded-md border border-slate-300 px-4 py-2 text-sm hover:bg-slate-100">
              {t('qr.generate')}
            </button>
            {qrError && <span className="text-sm text-amber-700">{qrError}</span>}
          </div>
        ) : (
          <div className="mt-3 flex flex-wrap items-start gap-6">
            {qrUrl && <img src={qrUrl} alt={t('qr.title')} width={256} height={256} className="rounded border border-slate-200" />}
            <div className="space-y-2 text-sm">
              <p className="break-all font-mono text-xs text-slate-600">{ticket.attendUrl}</p>
              <p className="text-slate-600">{t('qr.expires', { date: formatDateTime(ticket.expiresAt, lang) })}</p>
              <div className="flex gap-2">
                {qrUrl && (
                  <a href={qrUrl} download={`qr-${id}.png`} className="rounded-md border border-slate-300 px-3 py-1.5 hover:bg-slate-100">
                    {t('qr.download')}
                  </a>
                )}
                <button disabled={busy} onClick={onRegenerate} className="rounded-md border border-slate-300 px-3 py-1.5 hover:bg-slate-100">
                  {t('qr.regenerate')}
                </button>
              </div>
              {qrError && <p className="text-amber-700">{qrError}</p>}
            </div>
          </div>
        )}
      </section>

      <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <h2 className="font-bold">{t('attendance.manualTitle')}</h2>
        <form onSubmit={onManual} className="mt-3 grid max-w-xl gap-3">
          <select value={manual.userId} onChange={(e) => setManual({ ...manual, userId: e.target.value })} className={input}>
            <option value="">{t('attendance.pickStudent')}</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.fullName} · {u.controlNumber}
              </option>
            ))}
          </select>
          <div className="flex gap-3">
            <select value={manual.status} onChange={(e) => setManual({ ...manual, status: e.target.value })} className={input}>
              {STATUSES.map((s) => (
                <option key={s} value={s}>{t(`attendance.statuses.${s}`)}</option>
              ))}
            </select>
            <input value={manual.observation} onChange={(e) => setManual({ ...manual, observation: e.target.value })} placeholder={t('attendance.observation')} className={input} />
          </div>
          <button disabled={busy || !manual.userId} className="w-fit rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50">
            {t('common.save')}
          </button>
        </form>
      </section>

      <section>
        <h2 className="font-bold">{t('attendance.title', { count: rows.length })}</h2>
        {rows.length === 0 ? (
          <p className="mt-2 text-slate-600">{t('attendance.empty')}</p>
        ) : (
          <div className="mt-3 overflow-x-auto rounded-lg border border-slate-200 bg-white shadow-sm">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 text-slate-500">
                <tr>
                  <th className="px-4 py-2">{t('auth.controlNumber')}</th>
                  <th className="px-4 py-2">{t('auth.fullName')}</th>
                  <th className="px-4 py-2">{t('attendance.status')}</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((row) => (
                  <tr key={row.id} className="border-t border-slate-100">
                    <td className="px-4 py-2 font-mono">{row.controlNumber}</td>
                    <td className="px-4 py-2">{row.fullName}</td>
                    <td className="px-4 py-2">
                      <select
                        value={row.status}
                        disabled={busy}
                        onChange={(e) => onStatus(row, e.target.value)}
                        className="rounded-md border border-slate-300 px-2 py-1 text-xs"
                      >
                        {STATUSES.map((s) => (
                          <option key={s} value={s}>{t(`attendance.statuses.${s}`)}</option>
                        ))}
                      </select>
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
