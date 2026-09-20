import { Link } from 'react-router'
import { useTranslation } from 'react-i18next'
import LanguageSelector from './LanguageSelector.jsx'
import Icon from './Icon.jsx'
import LearningGraphic from './LearningGraphic.jsx'
import { MotionToggle } from './VisualEffects.jsx'

export default function AuthLayout({ mode, children }) {
  const { t } = useTranslation()
  return (
    <div className="auth-shell container-page">
      <header className="auth-toolbar">
        <Link to="/login" className="brand"><span className="brand-symbol"><Icon name="code" /></span>{t('app.title')}</Link>
        <div className="account-actions"><MotionToggle /><LanguageSelector /></div>
      </header>
      <main className="auth-composition">
        <section className="auth-editorial" aria-label={t('app.subtitle')}>
          <p className="eyebrow"><span className="status-dot" />{t('visual.workshop')}</p>
          <h2 className="editorial-title">{t('visual.headline')}<span className="gradient-text">{t('visual.headlineAccent')}</span></h2>
          <p className="editorial-description">{t('visual.introduction')}</p>
          <LearningGraphic />
          <div className="learning-pillars">
            {['learn', 'solve', 'advance'].map((key, index) => (
              <span key={key}><span className="pillar-number" aria-hidden="true">{`0${index + 1}`}</span>{t(`visual.${key}`)}</span>
            ))}
          </div>
        </section>
        <section className="auth-panel glass-panel" aria-labelledby="auth-title">
          <div className="auth-heading">
            <span className="section-icon"><Icon name={mode === 'login' ? 'lock' : 'profile'} /></span>
            <p className="eyebrow">{t(`visual.${mode}Eyebrow`)}</p>
            <h1 id="auth-title" className="page-title">{t(`auth.${mode}`)}</h1>
            <p className="text-muted">{t(`visual.${mode}Description`)}</p>
          </div>
          {children}
        </section>
      </main>
      <footer className="auth-footer"><Icon name="code" /><span>{t('app.subtitle')}</span></footer>
    </div>
  )
}
