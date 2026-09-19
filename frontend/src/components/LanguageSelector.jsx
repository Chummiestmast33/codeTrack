import { useTranslation } from 'react-i18next'
import { getLanguage, setLanguage } from '../i18n.js'

const LANGUAGES = [
  { code: 'es', label: 'ES' },
  { code: 'en', label: 'EN' },
]

export default function LanguageSelector() {
  const { t } = useTranslation()
  const current = getLanguage().split('-')[0]

  return (
    <label className="inline-flex items-center gap-2 text-sm text-slate-500">
      {t('app.languageLabel')}
      <select
        value={LANGUAGES.some((l) => l.code === current) ? current : 'es'}
        onChange={(e) => setLanguage(e.target.value)}
        className="rounded-md border border-slate-300 bg-white px-2 py-1 text-slate-700"
      >
        {LANGUAGES.map((l) => (
          <option key={l.code} value={l.code}>
            {l.label}
          </option>
        ))}
      </select>
    </label>
  )
}
