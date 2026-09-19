import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'

export default function Dashboard() {
  const { t } = useTranslation()
  const { user } = useAuth()

  return (
    <div>
      <h1 className="text-2xl font-bold">{t('dashboard.greeting', { name: user?.fullName ?? '' })}</h1>
      <p className="mt-2 text-slate-600">{t('dashboard.comingSoon')}</p>
    </div>
  )
}
