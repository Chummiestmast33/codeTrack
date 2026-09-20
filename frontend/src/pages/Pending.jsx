import { Link } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'
import Icon from '../components/Icon.jsx'

export default function Pending() {
  const { t } = useTranslation()
  const { logout } = useAuth()

  return (
    <main className="auth-page">
      <div className="auth-panel glass-panel text-center">
        <span className="section-icon"><Icon name="check" /></span>
        <p className="mt-6 text-sm text-muted">{t('visual.pendingEyebrow')}</p>
        <h1 className="mt-2 page-title">{t('auth.pendingTitle')}</h1>
        <p className="mt-2 text-muted">{t('auth.pendingNotice')}</p>
        <div className="mt-6 flex flex-wrap justify-center gap-3">
          <Link
            to="/login"
            className="btn font-medium text-ink hover:bg-raised"
          >
            {t('auth.login')}
          </Link>
          <button
            onClick={logout}
            className="btn btn-primary"
          >
            {t('auth.logout')}
          </button>
        </div>
      </div>
    </main>
  )
}
