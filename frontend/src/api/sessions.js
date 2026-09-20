import { api } from './client.js'

export const listSessions = () => api('/api/sessions')
export const getSession = (id) => api(`/api/sessions/${id}`)
export const createSession = (payload) => api('/api/sessions', { method: 'POST', body: payload })
export const updateSession = (id, payload) => api(`/api/sessions/${id}`, { method: 'PATCH', body: payload })
export const markSessionImparted = (id) => api(`/api/sessions/${id}/mark-imparted`, { method: 'POST' })
export const cancelSession = (id) => api(`/api/sessions/${id}/cancel`, { method: 'POST' })
