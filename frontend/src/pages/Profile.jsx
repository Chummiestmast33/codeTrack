import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'
import Icon from '../components/Icon.jsx'

export default function Profile() {
  const { t } = useTranslation()
  const { user } = useAuth()
  if (!user) return null

  return (
    <div>
      <h1 className="page-title">{t('profile.title')}</h1>
      <section className="profile-panel glass-panel">
        <div className="profile-intro">
          <span className="profile-avatar" aria-hidden="true">{user.fullName?.trim().slice(0, 1).toUpperCase()}</span>
          <p className="eyebrow">{t('visual.profileEyebrow')}</p>
          <h2>{user.fullName}</h2>
          <p className="text-muted">{t('visual.profileDescription')}</p>
        </div>
        <dl className="profile-details">
          {[['controlNumber', 'code'], ['fullName', 'profile'], ['email', 'mail']].map(([field, icon]) => (
            <div key={field}><dt><Icon name={icon} />{t(`auth.${field}`)}</dt><dd>{user[field]}</dd></div>
          ))}
        </dl>
      </section>
    </div>
  )
}
