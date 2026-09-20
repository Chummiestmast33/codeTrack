import { api } from './client.js'

export const listTopics = () => api('/api/topics')
export const getTopic = (id) => api(`/api/topics/${id}`)
export const createTopic = (payload) => api('/api/topics', { method: 'POST', body: payload })
export const updateTopic = (id, payload) => api(`/api/topics/${id}`, { method: 'PATCH', body: payload })
export const deactivateTopic = (id) => api(`/api/topics/${id}/deactivate`, { method: 'POST' })
export const activateTopic = (id) => api(`/api/topics/${id}/activate`, { method: 'POST' })
