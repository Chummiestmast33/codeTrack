import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'

export default function Profile() {
  const { t } = useTranslation()
  const { user } = useAuth()

  if (!user) return null

  return (
    <div>
      <h1 className="text-2xl font-bold">{t('profile.title')}</h1>
      <dl className="mt-4 max-w-md space-y-2 rounded-lg border border-slate-200 bg-white p-6 text-sm shadow-sm">
        <div className="flex justify-between gap-4">
          <dt className="text-slate-500">{t('auth.controlNumber')}</dt>
          <dd className="font-medium">{user.controlNumber}</dd>
        </div>
        <div className="flex justify-between gap-4">
          <dt className="text-slate-500">{t('auth.fullName')}</dt>
          <dd className="font-medium">{user.fullName}</dd>
        </div>
        <div className="flex justify-between gap-4">
          <dt className="text-slate-500">{t('auth.email')}</dt>
          <dd className="font-medium">{user.email}</dd>
        </div>
      </dl>
    </div>
  )
}
