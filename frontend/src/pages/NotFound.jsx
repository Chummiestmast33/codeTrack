import { Link } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'

export default function NotFound() {
  const { t } = useTranslation()
  const { token } = useAuth()

  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-50 px-6">
      <div className="text-center">
        <p className="text-6xl font-bold tracking-tight text-slate-300">404</p>
        <h1 className="mt-4 text-2xl font-bold text-slate-900">{t('notFound.title')}</h1>
        <p className="mt-2 text-slate-600">{t('notFound.description')}</p>
        <Link
          to={token ? '/app' : '/login'}
          className="mt-6 inline-block rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500"
        >
          {t('notFound.backHome')}
        </Link>
      </div>
    </main>
  )
}
