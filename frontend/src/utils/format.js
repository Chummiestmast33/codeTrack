/** Render backend ISO-8601 instants in the user's locale (RN-09: display only). */
export function formatDateTime(iso, lang = 'es') {
  if (!iso) return ''
  try {
    return new Intl.DateTimeFormat(lang, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso))
  } catch {
    return String(iso)
  }
}

export function formatDate(iso, lang = 'es') {
  if (!iso) return ''
  try {
    return new Intl.DateTimeFormat(lang, { dateStyle: 'medium' }).format(new Date(iso))
  } catch {
    return String(iso)
  }
}
