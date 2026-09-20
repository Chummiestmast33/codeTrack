import { api, getToken } from './client.js'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5245'

export const getAttendance = (sessionId) => api(`/api/sessions/${sessionId}/attendance`)
export const registerManualAttendance = (sessionId, payload) =>
  api(`/api/sessions/${sessionId}/attendance/manual`, { method: 'POST', body: payload })
export const updateAttendance = (id, payload) =>
  api(`/api/attendance/${id}`, { method: 'PATCH', body: payload })
export const registerByQr = (token) => api(`/api/attendance/qr/${token}`, { method: 'POST' })
export const getQr = (sessionId) => api(`/api/admin/sessions/${sessionId}/qr`)
export const regenerateQr = (sessionId) =>
  api(`/api/admin/sessions/${sessionId}/qr/regenerate`, { method: 'POST' })

/** <img> can't send Authorization headers: fetch the PNG with the token and expose a blob URL. */
export async function fetchQrImageUrl(sessionId) {
  const res = await fetch(`${API_URL}/api/admin/sessions/${sessionId}/qr/image`, {
    headers: { Authorization: `Bearer ${getToken()}` },
  })
  if (!res.ok) throw new Error(`QR image failed (${res.status})`)
  const blob = await res.blob()
  return URL.createObjectURL(blob)
}
