import { api } from './client.js'

export const listActivities = (topicId) =>
  api(`/api/activities${topicId ? `?topicId=${topicId}` : ''}`)
export const getActivity = (id) => api(`/api/activities/${id}`)
export const createActivity = (payload) => api('/api/activities', { method: 'POST', body: payload })
export const updateActivity = (id, payload) => api(`/api/activities/${id}`, { method: 'PATCH', body: payload })
export const publishActivity = (id) => api(`/api/activities/${id}/publish`, { method: 'POST' })
export const closeActivity = (id) => api(`/api/activities/${id}/close`, { method: 'POST' })
