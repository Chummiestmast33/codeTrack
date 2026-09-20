import { Link } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'

export default function NotFound() {
  const { t } = useTranslation()
  const { token } = useAuth()

  return (
    <main className="auth-page">
      <div className="auth-panel glass-panel text-center">
        <p className="text-6xl font-bold tracking-tight gradient-text">404</p>
        <h1 className="mt-4 page-title text-ink">{t('notFound.title')}</h1>
        <p className="mt-2 text-muted">{t('notFound.description')}</p>
        <Link
          to={token ? '/app' : '/login'}
          className="mt-6 inline-block btn btn-primary"
        >
          {t('notFound.backHome')}
        </Link>
      </div>
    </main>
  )
}
