import { Link } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'

export default function Pending() {
  const { t } = useTranslation()
  const { logout } = useAuth()

  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-50 px-6">
      <div className="w-full max-w-sm text-center">
        <h1 className="text-2xl font-bold">{t('auth.pendingTitle')}</h1>
        <p className="mt-2 text-slate-600">{t('auth.pendingNotice')}</p>
        <div className="mt-6 flex justify-center gap-3">
          <Link
            to="/login"
            className="rounded-md border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100"
          >
            {t('auth.login')}
          </Link>
          <button
            onClick={logout}
            className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500"
          >
            {t('auth.logout')}
          </button>
        </div>
      </div>
    </main>
  )
}
