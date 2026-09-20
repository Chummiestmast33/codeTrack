import { api, getToken } from './client.js'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5245'

export const listActivities = (topicId) =>
  api(`/api/activities${topicId ? `?topicId=${topicId}` : ''}`)
export const getActivity = (id) => api(`/api/activities/${id}`)
export const createActivity = (payload) => api('/api/activities', { method: 'POST', body: payload })
export const updateActivity = (id, payload) => api(`/api/activities/${id}`, { method: 'PATCH', body: payload })
export const publishActivity = (id) => api(`/api/activities/${id}/publish`, { method: 'POST' })
export const closeActivity = (id) => api(`/api/activities/${id}/close`, { method: 'POST' })

export const requestUploadTicket = (activityId, payload) =>
  api(`/api/activities/${activityId}/upload-ticket`, { method: 'POST', body: payload })
export const submitActivity = (activityId, payload) =>
  api(`/api/activities/${activityId}/submissions`, { method: 'POST', body: payload })
export const mySubmissionHistory = (activityId) => api(`/api/activities/${activityId}/submissions/me`)
export const submissionsByActivity = (activityId) => api(`/api/admin/activities/${activityId}/submissions`)
export const submissionFile = (id) => api(`/api/admin/submissions/${id}/file`)
export const reviewSubmission = (id, payload) =>
  api(`/api/admin/submissions/${id}/status`, { method: 'PATCH', body: payload })

/** Direct upload to object storage with the signed ticket (bytes never touch the API). */
export async function uploadToStorage(ticket, file) {
  const res = await fetch(ticket.uploadUrl, {
    method: 'PUT',
    headers: { 'Content-Type': file.type || 'application/octet-stream' },
    body: file,
  })
  if (!res.ok) throw new Error(`Storage upload failed (${res.status})`)
  return ticket.storagePath
}

export const myProgress = () => api('/api/progress/me')
export const studentProgress = (userId) => api(`/api/admin/progress?userId=${userId}`)
export const adjustProgress = (userId, topicId, payload) =>
  api(`/api/admin/progress/${userId}/${topicId}/adjust`, { method: 'POST', body: payload })
export const clearProgressAdjustment = (userId, topicId) =>
  api(`/api/admin/progress/${userId}/${topicId}/adjust`, { method: 'DELETE' })

/** Downloads a report blob (auth header required, like the QR image). */
export async function downloadReport(kind, format) {
  const res = await fetch(`${API_URL}/api/reports/${kind}?format=${format}`, {
    headers: { Authorization: `Bearer ${getToken()}` },
  })
  if (!res.ok) throw new Error(`Report download failed (${res.status})`)
  const disposition = res.headers.get('content-disposition') ?? ''
  const match = disposition.match(/filename="?([^";]+)"?/)
  const blob = await res.blob()
  return { url: URL.createObjectURL(blob), filename: match ? match[1] : `${kind}.${format}` }
}
