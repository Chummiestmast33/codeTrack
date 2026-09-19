import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import es from './locales/es.json'
import en from './locales/en.json'

const STORAGE_KEY = 'lang'
const DEFAULT_LANGUAGE = 'es'

function initialLanguage() {
  try {
    return localStorage.getItem(STORAGE_KEY) ?? DEFAULT_LANGUAGE
  } catch {
    return DEFAULT_LANGUAGE
  }
}

i18n.use(initReactI18next).init({
  resources: {
    es: { translation: es },
    en: { translation: en },
  },
  lng: initialLanguage(),
  fallbackLng: DEFAULT_LANGUAGE,
  interpolation: { escapeValue: false },
})

export function setLanguage(lang) {
  try {
    localStorage.setItem(STORAGE_KEY, lang)
  } catch {
    // private mode or unavailable storage: keep in-memory language only
  }
  return i18n.changeLanguage(lang)
}

export function getLanguage() {
  return i18n.language
}

export default i18n
