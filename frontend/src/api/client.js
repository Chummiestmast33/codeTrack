const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5245'
const TOKEN_KEY = 'token'

export function getToken() {
  try {
    return localStorage.getItem(TOKEN_KEY)
  } catch {
    return null
  }
}

export function setToken(token) {
  try {
    if (token) localStorage.setItem(TOKEN_KEY, token)
    else localStorage.removeItem(TOKEN_KEY)
  } catch {
    // private mode: session lives in memory only for this tab
  }
}

export class ApiError extends Error {
  constructor(status, title, detail, errors, code) {
    super(detail || title || `Request failed (${status})`)
    this.status = status
    this.title = title
    this.errors = errors
    this.code = code ?? null
  }
}

export async function api(path, { method = 'GET', body, token } = {}) {
  const headers = { 'Content-Type': 'application/json' }
  const auth = token ?? getToken()
  if (auth) headers.Authorization = `Bearer ${auth}`

  const res = await fetch(`${API_URL}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  })

  if (res.status === 204) return null

  let data
  try {
    data = await res.json()
  } catch {
    data = null
  }

  if (!res.ok) {
    throw new ApiError(
      res.status,
      data?.title,
      data?.detail,
      data?.errors ?? null,
      data?.code ?? null,
    )
  }

  return data
}
