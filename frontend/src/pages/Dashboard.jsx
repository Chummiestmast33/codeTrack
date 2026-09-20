import { Link } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'
import Icon from '../components/Icon.jsx'
import LearningGraphic from '../components/LearningGraphic.jsx'

export default function Dashboard() {
  const { t } = useTranslation()
  const { user } = useAuth()

  return (
    <div className="dashboard">
      <section className="dashboard-hero glass-panel">
        <div className="dashboard-intro">
          <p className="eyebrow">{t('dashboard.greeting', { name: user?.fullName ?? '' })}</p>
          <h1 className="hero-title">{t('visual.dashboardTitle')}<span className="gradient-text">{t('visual.dashboardAccent')}</span></h1>
          <p className="editorial-description">{t('visual.dashboardDescription')}</p>
          <Link to="/app/activities" className="btn btn-primary">{t('nav.activities')}<Icon name="arrow" /></Link>
        </div>
        <LearningGraphic />
      </section>
      <section aria-labelledby="explore-title">
        <div className="section-heading"><h2 id="explore-title">{t('visual.explore')}</h2><span className="section-line" /></div>
        <div className="destination-grid">
          {['sessions', 'activities', 'progress'].map((name, index) => (
            <Link key={name} to={`/app/${name}`} className={`destination-card destination-${name}`}>
              <div className="destination-top"><span className="section-icon"><Icon name={name} /></span><span className="destination-number" aria-hidden="true">{`0${index + 1}`}</span></div>
              <h3>{t(`nav.${name}`)}</h3>
              <p>{t(`visual.${name}Description`)}</p>
              <span className="destination-action">{t('visual.open')}<Icon name="arrow" /></span>
            </Link>
          ))}
        </div>
      </section>
    </div>
  )
}
